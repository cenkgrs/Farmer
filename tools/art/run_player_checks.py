"""Isolated Linux graphics test; requires Xvfb and xwininfo. No desktop focus changes."""
import argparse, ctypes as c, os, re, signal, subprocess, time
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--xvfb',default='Xvfb');p.add_argument('--name',required=True);p.add_argument('--checks',nargs='+',choices=['controls','farming','watering','art','tool-animation','building','world','house','roof'],default=['controls','farming','watering','art','tool-animation','building','world','house','roof']);args=p.parse_args()
root=Path(__file__).resolve().parents[2];os.chdir(root)
read,write=os.pipe()
server=subprocess.Popen([args.xvfb,'-displayfd',str(write),'-screen','0','1280x720x24','-nolisten','tcp'],pass_fds=(write,),stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
os.close(write)
player=None
try:
 display=':'+os.read(read,32).decode().strip();os.close(read)
 env=dict(os.environ,DISPLAY=display,LIBGL_ALWAYS_SOFTWARE='1',__GLX_VENDOR_LIBRARY_NAME='mesa')
 cmd=['./builds/Linux/Farmer.x86_64','-force-glcore','-screen-fullscreen','0','-screen-width','1280','-screen-height','720','--farmer-smoke-capture',f'builds/QA/{args.name}.png',*['--farmer-check-'+check for check in args.checks],'-logFile',str(root/'Logs'/f'{args.name}.log')]
 player=subprocess.Popen(cmd,env=env,stdout=subprocess.DEVNULL,start_new_session=True)
 x=c.CDLL('libX11.so.6');x.XOpenDisplay.argtypes=[c.c_char_p];x.XOpenDisplay.restype=c.c_void_p
 x.XSetInputFocus.argtypes=[c.c_void_p,c.c_ulong,c.c_int,c.c_ulong];x.XFlush.argtypes=[c.c_void_p];x.XCloseDisplay.argtypes=[c.c_void_p]
 connection=x.XOpenDisplay(display.encode());deadline=time.monotonic()+240
 while player.poll() is None and time.monotonic()<deadline:
  out=subprocess.check_output(['xwininfo','-root','-tree'],env=env,text=True)
  matches=re.findall(r'(0x[0-9a-f]+) "Farmer": \("Farmer.x86_64"',out)
  if len(matches)==1:
   x.XSetInputFocus(connection,int(matches[0],16),2,0);x.XFlush(connection)
  time.sleep(.5)
 x.XCloseDisplay(connection)
 if player.poll() is None:raise TimeoutError('Player did not finish within 240 seconds')
 print('PLAYER_EXIT',player.returncode,flush=True)
 raise SystemExit(player.returncode)
finally:
 try:
  if player is not None and player.poll() is None:
   os.killpg(player.pid, signal.SIGTERM)
   try: player.wait(timeout=5)
   except subprocess.TimeoutExpired:
    os.killpg(player.pid, signal.SIGKILL);player.wait(timeout=5)
 finally:
  server.terminate();server.wait(timeout=10)
