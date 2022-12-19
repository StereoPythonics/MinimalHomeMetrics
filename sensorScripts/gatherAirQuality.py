
#!/usr/bin/env python
# coding: utf-8

import argparse

from pms5003 import PMS5003
from itertools import islice
from datetime import datetime, timezone
import time
import os

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

    args = parseArguments()
    #Configure the PMS5003 for Enviro+
    pms5003 = PMS5003(
        device='/dev/ttyAMA0',
        baudrate=9600,
        pin_enable=22,
        pin_reset=27
    )
    
    source = args.source
    folder = args.metricStorage

    while True:
        try:
            data = pms5003.read()
            print(str(data.data))
            time = datetime.now(timezone.utc).isoformat()
            with open(os.path.join(folder,"air_q_pm1_0.json"),"w") as file0:
                file0.write(f'[{{"Name":"AirQualityPM1_0_ug_m-3","Value":{data.data[0]},"TimeStamp":"{time}","source":"{source}"}}]\n')
            with open(os.path.join(folder,"air_q_pm2_5.json"),"w") as file1:
                file1.write(f'[{{"Name":"AirQualityPM2_5_ug_m-3","Value":{data.data[1]},"TimeStamp":"{time}","source":"{source}"}}]\n')
            with open(os.path.join(folder,"air_q_pm10.json"),"w") as file2:
                file2.write(f'[{{"Name":"AirQualityPM10_ug_m-3","Value":{data.data[2]},"TimeStamp":"{time}","source":"{source}"}}]\n')
            print("Successful")
            break
        except KeyboardInterrupt:
             quit()
        except Exception as e: print(e)
    