import sys

# ruff: noqa: E402
sys.path.append("")

from micropython import const
from machine import Pin
import utime
import asyncio
import aioble
import bluetooth

import random
import struct

IsConnectedToPC = False
displayOrder = [[0, 0], [1, 0], [2, 0], [3, 0], [3, 1], [3, 2], [3, 3], [2, 3], [1, 3], [0, 3], [0, 4], [0, 5]] 
displayLoadingAnimationCounter = 0;

_NUMBERCOUNTER = 0;

# org.bluetooth.service.environmental_sensing
_ENV_SENSE_UUID = bluetooth.UUID(0x181A)
# org.bluetooth.characteristic.temperature
_ENV_SENSE_TEMP_UUID = bluetooth.UUID(0x2A6E)
# org.bluetooth.characteristic.rc_settings
_RC_SETTINGS_UUID = bluetooth.UUID(0x2B1E)
# org.bluetooth.characteristic.gap.appearance.xml
_ADV_APPEARANCE_GENERIC_THERMOMETER = const(768)

# How frequently to send advertising beacons.
_ADV_INTERVAL_MS = 250_000

BulbData = [[0, 0, 0, 0, 0, 0, 0, 0], [0, 0, 0, 0, 0, 0, 0, 0], [0, 0, 0, 0, 0, 0, 0, 0], [0, 0, 0, 0, 0, 0, 0, 0]]
segments = [0, 1, 2, 3, 4, 5, 6, 7]
pins = [Pin(seg, Pin.OUT) for seg in segments]

# Register GATT server.
temp_service = aioble.Service(_ENV_SENSE_UUID)
temp_characteristic = aioble.Characteristic(
    temp_service, _ENV_SENSE_TEMP_UUID, read=True, write=True, notify=True
)
settings_characteristic = aioble.Characteristic(
    temp_service, _RC_SETTINGS_UUID, read=True, write=True, notify=True
)
aioble.register_services(temp_service)


multiplexClockEnabled = Pin(15, Pin.OUT, value=0)
multiplexClock = Pin(14, Pin.OUT)
multiplexReset = Pin(13, Pin.OUT, value=0)

# This would be periodically polling a hardware sensor.

async def writeEvent():
    while True:
        
        connection = await temp_characteristic.written() #waits till someone writes to character
        
        ints = byteBufferToBinaryArray(temp_characteristic.read())
        displayTo7SegmentDisplay(ints)
        
        await asyncio.sleep_ms(50)

def byteBufferToBinaryArray(data):
    byteArray = []
    for b in data:
        bits = []
        for i in range(8): 
            bits.insert(0, (b >> i) & 1)
        byteArray.append(bits)
    return byteArray

def displayTo7SegmentDisplay(data):
    global BulbData
    BulbData = data

def displayNextData(pinUsed):
    global lastTasterPress
    if (utime.ticks_ms() - lastTasterPress > _TASTER_COOLDOWN):
        lastTasterPress = utime.ticks_ms()

     
        settings_characteristic.write(struct.pack("<h", 1), send_update=True)
        print("sent settings change")

# Serially wait for connections. Don't advertise while a central is
# connected.
async def peripheral_task():
    global IsConnectedToPC
    
    while True:
        IsConnectedToPC = False
        print("Waiting for connection")
        async with await aioble.advertise(
            _ADV_INTERVAL_MS,
            name="IV-11_DISPLAY",
            services=[_ENV_SENSE_UUID],
            appearance=_ADV_APPEARANCE_GENERIC_THERMOMETER,
        ) as connection:
            IsConnectedToPC = True
            print("Connection from", connection.device)
            await connection.disconnected(timeout_ms=None)


async def runBulbs():
    #set output pins for anodes
    while True:         
        multiplexReset.value(1)
        multiplexReset.value(0)
        
        for b in range(4):            
            for i in range(8):
                pins[i].value(BulbData[b][i])

            await asyncio.sleep_ms(2)
            
            multiplexClock.value(1)
            multiplexClock.value(0)
            
async def runStartAnimation():
    global BulbData
    while True:
        if (IsConnectedToPC == False):
            #not connected, show connecting animation
            for i in range(len(displayOrder)):
                if (IsConnectedToPC == True): continue
                BulbData = [[0, 0, 0, 0, 0, 0, 0, 0], [0, 0, 0, 0, 0, 0, 0, 0], [0, 0, 0, 0, 0, 0, 0, 0], [0, 0, 0, 0, 0, 0, 0, 0]]
                BulbData[displayOrder[i][0]][displayOrder[i][1]] = 1
                await asyncio.sleep_ms(100)
        else:
            await asyncio.sleep_ms(2000)
            
# Run all tasks.
async def main():
    t1 = asyncio.create_task(peripheral_task())
    t2 = asyncio.create_task(writeEvent())
    t3 = asyncio.create_task(runBulbs())
    t4 = asyncio.create_task(runStartAnimation())
    await asyncio.gather(t1, t2, t3, t4)


taster = Pin(22, Pin.IN)
taster.irq(trigger=Pin.IRQ_RISING, handler=displayNextData)
_TASTER_COOLDOWN = 500;
lastTasterPress = 0;


asyncio.run(main())