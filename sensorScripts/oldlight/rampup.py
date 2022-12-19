import sys
print(sys.path)
sys.path.append('/home/pi/.local/lib/python2.7/site-packages')
print(sys.path)
import wiringpi as wp

from time import sleep

wp.wiringPiSetupGpio()
wp.pinMode(12,1)
wp.pinMode(13,2)
sleep(1)

wp.digitalWrite(12,1)
for x in range(0,30):
	wp.pwmWrite(13,x)
	sleep(2)
for x in range(31,1024):
	wp.pwmWrite(13,x)
	sleep(0.05)
quit()
