import sys
print(sys.path)
sys.path.append('/home/pi/.local/lib/python2.7/site-packages')
print(sys.path)
import wiringpi as wp

from time import sleep

wp.wiringPiSetupGpio()
wp.pinMode(18,2)
sleep(1)


for x in range(0,1000):
	wp.pwmWrite(18,x)
	sleep(0.01)

sleep(120)
for x in range(1,1000):
	wp.pwmWrite(18,(999-x))
	sleep(0.01)

