import sys
print(sys.path)
sys.path.append('/home/pi/.local/lib/python2.7/site-packages')
print(sys.path)
import wiringpi as wp

from time import sleep

wp.wiringPiSetupGpio()
wp.pinMode(18,2)
wp.pinMode(27,1)

wp.digitalWrite(27,1)
sleep(1)

for x in range(1,1000):
	wp.pwmWrite(18,(1000-x))
	sleep(0.3)

wp.pwmWrite(18,0)
wp.digitalWrite(27,0)
quit()
