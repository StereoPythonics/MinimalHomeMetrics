import board
import busio
import digitalio
import time
import datetime
import adafruit_dht
from datetime import datetime, timezone
import os
import argparse

def parseArguments():
    # Create argument parser
    parser = argparse.ArgumentParser()

    # Positional mandatory arguments
    parser.add_argument("source", help="Which device is providing this metric?", type=str)
    parser.add_argument("metricStorage", help="Where should metric files be written?", type=str)

    # Parse arguments
    args = parser.parse_args()

    return args


if __name__ == '__main__':
    time.sleep(1)
    args = parseArguments()
    source = args.source
    folder = args.metricStorage
    retries = 0
    while True:
        try:
            dhtDevice = adafruit_dht.DHT11(board.D17)
            # Print the values to the serial port
            temperature_c = dhtDevice.temperature
            temperature_f = temperature_c * (9 / 5) + 32
            humidity = dhtDevice.humidity
            time = datetime.now(timezone.utc).isoformat()
            with open(os.path.join(folder,"temperature.json"),"w") as file0:
                file0.write(f'[{{"Name":"Temperature","Value":{temperature_c},"TimeStamp":"{time}","source":"{source}"}}]\n')
            with open(os.path.join(folder,"humidity.json"),"w") as file1:
                file1.write(f'[{{"Name":"Humidity","Value":{humidity},"TimeStamp":"{time}","source":"{source}"}}]\n')
            print(f'successful {humidity}% {temperature_c}C')
            
            break

        except KeyboardInterrupt:
             quit()

        except Exception as error:
            # Errors happen fairly often, DHT's are hard to read, just keep going
            print(error.args[0])
            time.sleep(0.5)
            retries = retries + 1
            if(retries > 30):
                print("giving up")
                break
        dhtDevice.exit()

    
    