## 구매 해야 될 품목
-MCP2515<br>
-점퍼선<br>
-SD카드<br>
-라즈베리파이 4 hdmi(마이크로)<br>
-디스플레이(나중에, 일단 포터블모니터 사용)


## MCP2515 구매 경로
- https://www.smart3dkr.com/product/MCP2315
- https://vctec.co.kr/product/can-%EB%B2%84%EC%8A%A4-%EC%BB%A8%ED%8A%B8%EB%A1%A4%EB%9F%AC-%EB%AA%A8%EB%93%88-spi-mcp2515-can-bus-controller-module-spi-mcp2515/10537/
- https://parts-parts.co.kr/product/pp-a233-mcp2515-can-%ED%86%B5%EC%8B%A0-%EB%AA%A8%EB%93%88-can-bus-module/207/
## MCP 2515 obd2 플러그 연결
https://www.youtube.com/watch?v=cAAzXM5vsi0 10:52초
## MCP 2515 라즈베리파이 연결
- https://memories.tistory.com/129
(위와 같은 모듈)

<details>
  <summary>
    라즈베리파이-MCP2515 핀맵 ( 라즈베리파이 버전 확인 필요 )
  </summary>

  ![img](https://img1.daumcdn.net/thumb/R1280x0/?scode=mtistory2&fname=https%3A%2F%2Fblog.kakaocdn.net%2Fdn%2FcnPQRU%2Fbtq2ggaGiUc%2FDCk9gK3ZKfuUhc4znoY7S1%2Fimg.jpg)
</details>

## python mcp2515 라이브러리
https://docs.circuitpython.org/projects/mcp2515/en/latest/

(Simple test)https://docs.circuitpython.org/projects/mcp2515/en/latest/examples.html

<details>
  <summary>adafruit_mcp2515 예제</summary>
  
  ### examples/mcp2515_simpletest.py
  

  ### python code
```python
# SPDX-FileCopyrightText: Copyright (c) 2020 Bryan Siepert for Adafruit Industries
#
# SPDX-License-Identifier: MIT
from time import sleep
import board
import busio
from digitalio import DigitalInOut
from adafruit_mcp2515.canio import Message, RemoteTransmissionRequest
from adafruit_mcp2515 import MCP2515 as CAN


cs = DigitalInOut(board.D5)
cs.switch_to_output()
spi = busio.SPI(board.SCK, board.MOSI, board.MISO)

can_bus = CAN(
   spi, cs, loopback=True, silent=True
)  # use loopback to test without another device
while True:
   with can_bus.listen(timeout=1.0) as listener:

       message = Message(id=0x1234ABCD, data=b"adafruit", extended=True)
       send_success = can_bus.send(message)
       print("Send success:", send_success)
       message_count = listener.in_waiting()
       print(message_count, "messages available")
       for _i in range(message_count):
           msg = listener.receive()
           print("Message from ", hex(msg.id))
           if isinstance(msg, Message):
               print("message data:", msg.data)
           if isinstance(msg, RemoteTransmissionRequest):
               print("RTR length:", msg.length)
   sleep(1)
```
### 해결해야 할 것들
```python
message = Message(id=0x1234ABCD, data=b"adafruit", extended=True)
#여기서 id와 data를 뭘 써야 할까?
11bit ID 0x7DF로 요청을 보내면 ID 0x7E8 또는 0x7E9로 응답이 온다.
29bit ID Request : 0x18DAF110, Response : 0x18DB33F1
```
</details>

<details>
  <summary>
  RAW OBD2 frame details
  </summary>


  비교용으로 사진 2개 가지고 옴

  https://www.redalyc.org/journal/4115/411556006003/html/

  ![img](https://www.redalyc.org/journal/4115/411556006003/411556006003_gf18.png)

  https://www.csselectronics.com/pages/obd2-explained-simple-intro


  ![img](https://canlogger1000.csselectronics.com/img/OBD2-frame-raw-mode-PID-ID-bytes.svg)

  - Identifier: For OBD2 messages, the identifier is standard 11-bit and used to distinguish between "request messages" (ID 7DF) and "response messages" (ID 7E8 to 7EF). Note that 7E8 will typically be where the main engine or ECU responds at.

  - Length: This simply reflects the length in number of bytes of the remaining data (03 to 06). For the Vehicle Speed example, it is 02 for the request (since only 01 and 0D follow), while for the response it is 03 as both 41, 0D and 32 follow.
  
  __요청시에는 Mode,PID만 쓰니까 0x02, 보낼때는 만약 A,B 쓴다고 하면 4개니까 0x04__

  - Mode: For requests, this will be between 01-0A. __For responses the 0 is replaced by 4__ (i.e. 41, 42, … , 4A). There are 10 modes as described in the SAE J1979 OBD2 standard. Mode 1 shows Current Data and is e.g. used for looking at real-time vehicle speed, RPM etc. Other modes are used to e.g. show or clear stored diagnostic trouble codes and show freeze frame data.

  - PID: For each mode, a list of standard OBD2 PIDs exist - e.g. in Mode 01, PID 0D is Vehicle Speed. For the full list, check out our OBD2 PID overview. Each PID has a description and some have a specified min/max and conversion formula.

  - The formula for speed is e.g. simply A, meaning that the A data byte (which is in HEX) is converted to decimal to get the km/h converted value (i.e. 32 becomes 50 km/h above). For e.g. RPM (PID 0C), the formula is (256*A + B) / 4.

  - A, B, C, D: These are the data bytes in HEX, which need to be converted to decimal form before they are used in the PID formula calculations. Note that the last data byte (after Dh) is not used.

  ![OBD2 request/response example](https://cdn.shopify.com/s/files/1/0579/8032/1980/files/obd2-pid-request-response-7df-7e8-vehicle-speed.svg?v=1633690039)


  <details>
    <summary>
    Extended OBD2 PID request/response
    </summary>
    In some vehicles (e.g. vans and light/medium/heavy duty vehicles), you may find that the <u>raw CAN data uses extended 29-bit CAN identifiers instead of 11-bit CAN identifiers.</u>

    In this case, you will typically need to modify the OBD2 PID requests to use the <u>CAN ID 18DB33F1 instead of 7DF.</u> The data payload structure is kept identical to the examples for 11-bit CAN IDs.

    If the vehicle responds to the requests, you'll typically see responses with CAN IDs 18DAF100 to 18DAF1FF (in practice, typically 18DAF110 and 18DAF11E). The response identifier is also sometimes shown in the 'J1939 PGN' form, specifically the PGN 0xDA00 (55808), which in the J1939-71 standard is marked as 'Reserved for ISO 15765-2'.

    We provide an OBD2 DBC file for both the 11-bit and 29-bit responses, enabling simple decoding of the data in most CAN software tools.
  </details>

  ![OBD mode - SAE J1979](https://cdn.shopify.com/s/files/1/0579/8032/1980/files/obd2-10-pid-modes-diagnostic-services.svg?v=1633690039)
  
</details>

### OBD-2 PID List는 어디서?
- https://en.wikipedia.org/wiki/OBD-II_PIDs(표준 PID list)
-  https://gsw.hyundai.com (일반인 가입 가능) - 아직 PID list 찾지는 못함<br>
- PID는 OBD-II 에서 정의되지 않고, SAE J1979에서 정의되엇다.<br>
- 현대자동차와 다른 자동차 제조업체들은 OBD2 (On-Board Diagnostics 2) 시스템을 구현할 때 SAE J1979 표준을 따른다. (SAE J1979는 OBD2 시스템에서 사용되는 PID (Parameter ID) 값, 데이터 형식, 프로토콜 등을 정의한 표준입니다.)
따라서 현대자동차는 일반적으로 SAE J1979 표준을 준수하여 OBD2 시스템을 구현하고, PID 값 및 데이터 형식을 표준에 맞게 사용한다. 그러나 각 제조업체는 SAE J1979 표준을 기반으로 하되, 추가적인 제조사 특정 PID 값이나 확장된 기능을 제공할 수도 있습니다.

- 고장진단코드 (0x03)을 받는 방법은 
https://en.wikipedia.org/wiki/OBD-II_PIDs#cite_note-formula-3 에서
Service 03 - Show stored Diagnostic Trouble Codes (DTCs)
을 참조.
(Formula와 see below 링크를 참고, 대체 n은 뭘 의미하는가?)


<br><br>
# 우리의 라즈베리파이는?
- 현재 소유 중: 라즈베리파이4 B 4GB
- 8GB가 필요할 수도..
<details>
  <summary>
    Raspberry Pi 4 GPIO Pinout
  </summary>
  https://linuxhint.com/gpio-pinout-raspberry-pi/

  ![img](https://linuxhint.com/wp-content/uploads/2022/02/image6-34.png)
</details>

<br>

# ELM-327을 활용하는 방법도 있다

- https://python-obd.readthedocs.io/en/latest/

<br>

# OBD2 MCP2515 CPP example code

https://github.com/yogiD/MCP2515-OBD/blob/master/mcp_can.cpp


???
https://buildmedia.readthedocs.org/media/pdf/python-can/develop/python-can.pdf

examples/OBD2_PID_Request/OBD2_PID_Request.ino 참조
(mcp_can.cpp 를 사용)

https://github.com/coryjfowler/MCP_CAN_lib/tree/master



# 현대,기아 DTC list
https://cafe.naver.com/ittec/104
<details>
    <summary>
    KIA DTC List
    </summary>


    Kia OBD-II Trouble Codes 

    P1115 Engine Coolant Temperature Signal from ECM to TCM. 
    P1121 Throttle Position Sensor Signal Malfunction from ECM to TCM. 
    P1170 Front Heated Oxygen Sensor Stuck. 
    P1195 EGR Pressure Sensor (1.6L) or Boost Sensor (1.8L) Open or Short. 
    P1196 Ignition Switch "Start" Open or Short (1.6L). 
    P1250 Pressure Regulator Control Solenoid Valve Open or Short. 
    P1252 Pressure Regulator Control Solenoid Valve No. 2 Circuit Malfunction. 
    P1307 Chassis Acceleration Sensor Signal Malfunction. 
    P1308 Chassis Acceleration Sensor Signal Low. 
    P1309 Chassis Acceleration Sensor Signal High. 
    P1345 No SGC Signal (1.6L). 
    P1386 Knock Sensor Control Zero Test. 
    P1402 EGR Valve Position Sensor Open or Short. 
    P1449 Canister Drain Cut Valve Open or Short (1.8L). 
    P1450 Excessive Vacuum Leak. 
    P1455 Fuel Tank Sending Unit Open or Short (1.8L). 
    P1457 Purge Solenoid Valve Low System Malfunction. 
    P1458 A/C Compressor Control Signal Malfunction. 
    P1485 EGR Solenoid Valve Vacuum Open or Short. 
    P1486 EGR Solenoid Valve Vent Open or Short. 
    P1487 EGR Boost Sensor Solenoid Valve Open or Short. 
    P1496 EGR Stepper Motor Malfunction - Circuit 1 (1.8L). 
    P1497 EGR Stepper Motor Malfunction - Circuit 2 (1.8L). 
    P1498 EGR Stepper Motor Malfunction - Circuit 3 (1.8L). 
    P1499 EGR Stepper Motor Malfunction - Circuit 4 (1.8L). 
    P1500 No Vehicle Speed Signal to TCM. 
    P1505 Idle Air Control Valve Opening Coil Voltage Low. 
    P1506 Idle Air Control Valve Opening Coil Voltage High. 
    P1507 Idle Air Control Valve Closing Coil Voltage Low. 
    P1508 Idle Air Control Valve Closing Coil Voltage High. 
    P1523 VICS Solenoid Valve. 
    P1586 A/T-M/T Codification. 
    P1608 PCM Malfunction. 
    P1611 MIL Request Circuit Voltage Low. 
    P1614 MIL Request Circuit Voltage High. 
    P1624 MIL Request Signal from TCM to ECM. 
    P1631 Alternator "T" Open or No Power Output (1.8L). 
    P1632 Battery Voltage Detection Circuit for Alternator Regulator (1.8L). 
    P1633 Battery Overcharge. 
    P1634 Alternator "B" Open (1.8L). 
    P1693 MIL Circuit Malfunction. 
    P1743 Torque Converter Clutch Solenoid Valve Open or Short. 
    P1794 Battery or Circuit Failure. 
    P1795 4WD Switch Signal Malfunction. 
    P1797 P or N Range Signal or Clutch Pedal Position Switch Open or Short. 
</details>
<details>
    <summary>
    Hyundai DTC List
    </summary>



    Hyundai OBD-II Trouble Codes

    P1100 Map Sensor - Malfunction
    P1101 Map Sensor - Abnormal
    P1102 Map Sensor - Low Input
    P1103 Map Sensor - High Input
    P1104 Air Flow
    P1105 Air Flow - Abnormal
    P1106 Air Flow - Low Input
    P1107 Air Flow - High Input
    P1108 Fuel Pump
    P1109 Fuel Pump - Abnormal
    P1110 Fuel Pump - Stuck On
    P1110 ETS System - Malfunction
    P1111 Fuel Pump - Electrical
    P1112 Manifold Differential Pressure Sensor
    P1112 VGT Actuator - Malfunction
    P1113 Manifold Differential Pressure Sensor - Abnormal
    P1114 Manifold Differential Pressure Sensor - Low
    P1115 Coolant Temperature Input - Abnormal
    P1116 Boost Pressure Sensor - Malfunction
    P1118 ETS Motor - Malfunction
    P1119 Inlet Metering Valve Control
    P1120 Electric Governor - Malfunction
    P1120 Inlet Metering Valve Malfunction
    P1121 APS PWM Output Circuit Malfunction
    P1121 Throttle Position Input - Abnormal
    P1122 Boost Pressure Control Valve
    P1123 Fuel System Rich - Idle
    P1123 Timer Position Sensor - Malfunction
    P1124 Fuel System Lean - Idle
    P1125 Fuel Press Sensor1
    P1126 Fuel Press Sensor2
    P1127 Fuel System Rich - Part Load
    P1127 Control Sleeve Position Sensor
    P1128 Fuel System Lean - Part Load
    P1130 Start Solenoid- Malfunction
    P1131 Injection Quantity Adjust
    P1134 O2s Transition Time(B1/S1)
    P1135 Injection Timing Servo
    P1140 Inlet Air Temperature Sensor Malfunction
    P1141 Slow Duty Solenoid- Malfunction
    P1145 Main Duty Solenoid- Malfunction
    P1146 Idle Co Potentiometer
    P1147 Accelerator Position Sensor (ETS) Circuit
    P1150 Barometric Pressure Sensor Malfunction
    P1151 Accelerator Position Sensor (EMS) Circuit
    P1152 Accelerator Position Circuit-Low Input
    P1153 Accelerator Position Circuit-High Input
    P1154 O2s Transition Time(B2/S1)
    P1155 Limp Home Valve- Malfunction
    P1159 Variable Induction System
    P1162 High Pressure Pump & Fuel Line
    P1166 O2s (B1) Control Adaptation
    P1166 Limit O2s Lambda Control (B1)
    P1167 O2s (B2) Control Adaptation
    P1167 Limit O2s Lambda Control (B2)
    P1168 O2s (B1/S2) Heater Power
    P1169 O2s (B2/S2) Heater Power
    P1170 ECM (Barometric Pressure Sensor)
    P1171 ETS Valve Stuck - Open
    P1172 ETS Improper Motor Current
    P1173 ETS Target Following Malfunction
    P1174 ETS Valve Stuck - Close #1
    P1175 ETS Valve Stuck - Close #2
    P1176 ETS Motor Open/Short #1
    P1177 ETS Motor Open/Short #2
    P1178 ETS Motor Power - Open
    P1179 ETS Position F/B-Mismatch
    P1180 O2 S1 Heater Circuit- Malfunction
    P1180 Fuel Pressure Regulator - Malfunction
    P1181 Fuel Pressure Monitoring
    P1182 O2 S2 Heater Circuit - Malfunction
    P1182 Fuel Pressure Regulator - Short
    P1183 Fuel Pressure Regulator - Open
    P1184 O2s No Activity (B1/S2)
    P1184 Fuel Pressure Regulator - Power
    P1185 Fuel Position - Excessive
    P1186 Fuel Pressure - Too Low
    P1187 Regulator Valve - Stuck
    P1188 Fuel Pressure - Leakage
    P1189 Governor Deviation
    P1190 Intake Throttle Actuator
    P1191 ETS Limp Home Valve On
    P1192 Limp home - Target Follow Malfunction
    P1193 ETS Limp Home - Low Rpm
    P1194 Limp Home - TPS2 Position Malfunction
    P1195 Limp Home - Target Follow Delay
    P1196 ETS Limp Home - Close Stuck
    P1300 Spark Timing Adjust Signal
    P1300 Injector Specific Data Fault
    P1300 Synchronization Error-CKP/CMP
    P1301 TDC Sensor - Abnormal
    P1302 TDC Sensor - Low Input
    P1303 TDC Sensor - High Input
    P1304 Phase Sensor
    P1305 Phase Sensor - Abnormal
    P1306 Phase Sensor - Low Input
    P1307 Phase Sensor - High Input
    P1307 Accelerator Sensor - Range/Performance
    P1308 Ignition Coil.1
    P1308 Accelerator Sensor - Low
    P1308 Accelerator Sensor Circuit - Low
    P1309 Ignition Coil.1 - Abnormal
    P1309 Accelerator Sensor - High
    P1309 Accelerator Sensor Circuit - High
    P1310 Ignition Coil.1 - Low Output
    P1310 Injection Control Circuit Fault
    P1311 Ignition Coil.1 - High Output
    P1312 Ignition Coil.2
    P1313 Ignition Coil.2 - Abnormal
    P1314 Ignition Coil.2 - Low Output
    P1315 Ignition Coil.2 - High Output
    P1316 Ignition Coil.3
    P1317 Ignition Coil.3 - Abnormal
    P1318 Ignition Coil.3 - Low Output
    P1319 Ignition Coil.3 - High Output
    P1320 Ignition Coil.4
    P1321 Ignition Coil.4 - Abnormal
    P1321 Glow Indicator Lamp - Short
    P1322 Ignition Coil.4 - Low Output
    P1322 Glow Indicator Lamp - Open
    P1323 Ignition Coil.4 - High Output
    P1324 Glow Relay - Malfunction
    P1325 Glow Relay - Abnormal
    P1325 Glow Relay
    P1326 Glow Relay - Stuck On
    P1326 Glow Relay - Short
    P1327 Glow Relay - Electrical
    P1327 Glow Relay - Open
    P1330 Spark Timing Adjust Signal
    P1331 #1 MF Signal Line Short
    P1332 #2 MF Signal Line Short
    P1333 #3 MF Signal Line Short
    P1334 #4 MF Signal Line Short
    P1335 #5 MF Signal Line Short
    P1336 #6 MF Signal Line Short
    P1337 #7 MF Signal Line Short
    P1338 #8 MF Signal Line Short
    P1340 IFS 2 Open
    P1341 #1 Ion Line Open
    P1342 #2 Ion Line Open
    P1343 #3 Ion Line Open
    P1344 #4 Ion Line Open
    P1345 #5 Ion Line Open
    P1346 #6 Ion Line Open
    P1347 #7 Ion Line Open
    P1348 #8 Ion Line Open
    P1351 #1 MF Signal Line Open
    P1352 #2 MF Signal Line Open
    P1353 #3 MF Signal Line Open
    P1354 #4 MF Signal Line Open
    P1355 #5 MF Signal Line Open
    P1356 #6 MF Signal Line Open
    P1357 #7 MF Signal Line Open
    P1358 #8 MF Signal Line Open
    P1372 Segment Time Incorrect
    P1400 Manifold Differential Pressure Sensor
    P1401 DMTL Performance
    P1402 DMTL Motor Circuit
    P1403 DMTL Valve Circuit
    P1404 DMTL Heater Circuit
    P1405 EGR Temperature Incorrect
    P1415 2nd Air Injection Insufficient (B1)
    P1416 Secondary Air Injection
    P1418 2nd Air Injection Insufficient (B2)
    P1419 2nd Air Injection Valve
    P1440 EVAP System-Vent Circuit
    P1440 Canister Valve Open/Short
    P1443 EVAP System - Tank Cap Missing
    P1458 A/C Switch Malfunction
    P1500 A/Con Switch
    P1500 Vehicle Speed Signal Malfunction
    P1500 Vehicle Speed Input - Abnormal
    P1501 Brake Switch
    P1502 Wheel Speed Sensor Circuit Malfunction
    P1503 Accelerator Switch
    P1503 Auto Cruise Control Switch
    P1504 Inhibitor Switch
    P1504 Auto Cruise Cancel
    P1505 Kick-Down Servo Switch
    P1505 Idle Speed Actuator (Open) - Open
    P1506 MPS
    P1506 Idle Speed Actuator (Open) - Short
    P1507 MPS - Abnormal
    P1507 Idle Speed Actuator (Close) - Open
    P1508 MPS - Low
    P1508 Idle Speed Actuator (Close) - Short
    P1509 MPS - High
    P1510 Idle Speed Actuator (Open) - Short
    P1510 ISC Circuit Open/Short
    P1511 ISC Open/Short Coil2
    P1511 ISC Circuit Open/Short
    P1512 WTS - Low
    P1513 Idle Speed Actuator (Open) - Open
    P1514 Oil Temperature Sensor
    P1515 Oil Temperature Sensor - Abnormal
    P1515 ISA Coil1 Command Sig. Malfunction
    P1516 Oil Temperature Sensor - Low
    P1516 ISA Coil2 Command Sig. Malfunction
    P1517 Oil Temperature Sensor - High
    P1520 Generator Front Terminal Malfunction
    P1521 Power Steering Switch Circuit
    P1522 Battery
    P1522 Sensor Supply 1 Volt - Low
    P1523 Sensor Supply 1 Volt - High
    P1524 Sensor Supply 2 Volt - Low
    P1525 5v Source Voltage
    P1525 Sensor Supply 2 Volt - High
    P1526 Sensor Supply Voltage1 - Malfunction
    P1527 Sensor Supply Voltage2 - Malfunction
    P1529 TCM Mil On Request Signal
    P1529 Check TCM
    P1530 Maximum Vehicle Speed Limiting
    P1543 Brake Switch Signal Fault
    P1545 A/C Comp. Switch Open/Short
    P1552 Idle Speed Act.(Close) - Short
    P1553 Idle Speed Act.(Close) - Open
    P1555 Idle Co-Potentiometer Open/Short
    P1567 Fuel Pressure Valve Current - Low
    P1568 Fuel Pressure Valve Current - Hi
    P1569 Current-Pressure Control Valve
    P1569 Press Control Valve - Current
    P1586 MT/AT Encoding Sig. Error
    P1600 Serial Communication - Malfunction (With4A/T)
    P1601 ECM ‘L’ Line
    P1602 ECU-TCU Communication Line #1 Malfunction
    P1603 ECU-TCU Communication Line #2 Malfunction
    P1603 ECM-TCSCM Communication Line
    P1603 TCU Error
    P1603 CAN Communication Bus Off
    P1604 TPS Communication Malfunction
    P1604 No ID From ECU
    P1605 Rough Road Sensor Malfunction
    P1605 Acceleration Sensor - Malfunction
    P1606 Rough Road Sensor Not Ration
    P1606 Acceleration Sensor - Abnormal
    P1607 Vacuum Sol. Valve Stuck On
    P1607 ECM-ETS Communication Line - Malfunction
    P1607 ECM-ETS Communication Line - Malfunction
    P1608 Vacuum Solenoid Valve Electrical
    P1608 ECU Fault
    P1608 ETS-ECM Communication Line - Malfunction
    P1609 Immobilizer Communication - Malfunction
    P1610 Immobilizer - Smartra Error
    P1610 Sensor Reference Voltage Fault
    P1611 Mil Request Signal Low
    P1611 Immobilizer - Transponder Malfunction
    P1612 Ventilation Solenoid Valve Elec.
    P1612 Immobilizer - Smartra Malfunction
    P1613 ECM-Failure
    P1613 Immobilizer Communication Error
    P1613 Mil Request Line - High
    P1613 ECM-Malfunction
    P1613 ECM (Voltage Regulator) - Malfunction
    P1614 Mil Request Signal High
    P1614 ECU Software Fault
    P1614 ETS ECU - Malfunction
    P1614 ETS ECU - Malfunction
    P1615 ETS - ECM Malfunction
    P1616 Main Relay Malfunction
    P1619 Main Relay
    P1620 A/C Relay Malfunction
    P1620 A/C Comp. Relay - Short
    P1621 Fuel Cut Valve - Malfunction
    P1621 A/C Comp. Relay - Open
    P1622 A/C Control Circuit Failure
    P1622 A/C Relay - Malfunction
    P1622 A/C Comp. Relay - Malfunction
    P1623 Mil Open/Short
    P1623 Engine Check Lamp - Malfunction
    P1623 Radiator Cooling Fan - Malfunction
    P1624 Mil-On Request From TCM
    P1624 Cooling Fan Relay (Low) Cir
    P1624 Radiator Cooling Fan - Malfunction
    P1624 Cooling Fan Relay - Low
    P1625 A/C Condenser Fan - Malfunction
    P1625 Cooling Fan Relay (High) Circuit
    P1625 Cooling Fan Relay - High
    P1626 Immobilizer Indicator Malfunction
    P1627 A/C Condenser Fan
    P1628 Condenser Fan Circuit - Low
    P1629 Glow Indicator Lamp - Malfunction
    P1629 Condenser Fan Circuit - High
    P1630 CAN-Bus-Off
    P1630 Intercooler Fan - Malfunction
    P1631 Serial Communication Error - Password
    P1631 CAN-Time Out ECU
    P1632 TCS System Malfunction
    P1632 CAN Bus Off
    P1633 Immobilizer Status Lamp
    P1634 Cruise Control Lamp - Malfunction
    P1634 Auxiliary Heat Relay - Short/Open
    P1635 Water Heater Relay - Malfunction
    P1636 Voltage Regulator For Inject.
    P1638 ECM (Microcontroller) - Malfunction
    P1639 ECM (Monitoring ADC) - Malfunction
    P1640 VI Motor Open/Short Coil1
    P1640 Main Relay Malfunction
    P1641 VI Motor Open/Short Coil2
    P1642 Non Immobilizer ECU Equipped
    P1645 Booster Voltage - Operate Injector
    P1646 Booster Voltage - Too High
    P1647 Threshold 1 Voltage - Malfunction
    P1647 Booster Voltage -Too Low
    P1652 Ignition Switch - Malfunction
    P1653 After-Run Check Error
    P1660 Cruise Control Switch - Malfunction
    P1665 Power Stage Group A Malfunction.
    P1670 Power Stage Group B Malfunction
    P1670 Injector Classification
    P1672 Fan Relay Malfunction - Low Speed
    P1673 Fan Relay Malfunction - High Speed
    P1674 A/C Fan Relay Malfunction
    P1690 Immobilizer Smartra Error
    P1690 W/Heater Relay Fault
    P1691 Immobilizer Antenna Error
    P1692 Immobilizer - Indicator Lamp Error
    P1693 Mil Request Line - Malfunction
    P1693 Immobilizer Transponder
    P1694 Immobilizer ECU Signal Error
    P1695 Immobilizer EEPROM Error
    P1696 Immobilizer - Key Mismatched
    P1697 Invalid Tester Request
    P1698 Key ID. Not Valid
    P1700 A/T Gear Shift Malfunction
    P1701 TPS
    P1702 TPS - Abnormal
    P1702 TPS - Malfunction Adjustment
    P1702 TPS - Malfunction Adjustment
    P1703 TPS - Low Input
    P1703 TPS - Open/Short (Ground)
    P1703 TPS - Open/Short (Ground)
    P1704 TPS - High Input
    P1704 TPS - Short
    P1704 TPS - Short
    P1706 Inhibitor Switch
    P1707 Brake Switch
    P1707 Auto Cruise Brake Switch
    P1708 Accelerator Switch
    P1709 Kick-Down Servo Switch
    P1709 K/D Servo Switch - Open/Short
    P1710 Oil Temperature Sensor
    P1711 Oil Temperature Sensor Abnormal
    P1712 Oil Temperature Sensor Low Input
    P1713 Oil Temperature Sensor High Input
    P1714 Idle Switch
    P1715 A/C on Switch
    P1716 Steering Sensor
    P1717 Steering Sensor Abnormal
    P1717 Steer 1 Input Signal
    P1718 Steering Sensor Low Input
    P1718 Steer 2 Input Signal
    P1719 Steering Sensor High Input
    P1719 Steer N Input Signal
    P1721 Control Relay
    P1722 Control Relay - Abnormal
    P1723 Control Relay - Stuck On
    P1723 A/T Relay - Open/Short (Ground)
    P1723 A/T Relay - Open/Short (Ground)
    P1724 Control Relay - Electrical
    P1725 TOD Control Module Error
    P1726 TPS Input - Loss Of Signal
    P1727 TPS Input - Out Of Range
    P1728 EMC-Open/Short To Battery
    P1729 EMC-Short To Ground
    P1730 Front Speed Sensor - Low Input
    P1731 Front Speed Sensor - High
    P1732 Rear Speed Sensor - Low Input
    P1733 Rear Speed Sensor - High Input
    P1734 Speed Sensor Reference - Low
    P1735 Speed Sensor Reference - High
    P1736 Shift Motor - Open
    P1737 ECV Stick
    P1737 Shift Motor - Short To Ground
    P1738 Shift System Time Out
    P1739 General Position Encoder Fault
    P1740 Position1 - Short To Ground
    P1741 Position2 - Short To Ground
    P1742 Position3 - Short To Ground
    P1743 Position4 - Short To Ground
    P1744 Lock Up Clutch System
    P1744 Anomalous Vibration Occurrence
    P1745 TCM ‘K’ Line
    P1746 TCM ‘L’ Line
    P1747 TCU-ECU Communication Line #1 Malfunction
    P1748 TCU-ECU Communication Line2 Malfunction
    P1749 Serial Communication Link
    P1749 Serial Communication Link-Open/Short
    P1749 Serial Communication Link-Open/Short
    P1750 TPS Communication Malfunction
    P1750 Front Left Speed Sensor
    P1751 Not Defined DTC
    P1751 Front Right Speed Sensor
    P1752 Lamp ‘P’
    P1752 Rear Left Speed Sensor
    P1753 Lamp ‘R’
    P1753 Rear Right Speed Sensor
    P1754 Lamp ‘N’
    P1755 Lamp ‘D’
    P1756 Lamp ‘3’
    P1757 Lamp ‘2’
    P1758 Lamp ‘L’
    P1759 Control Module - Check Sum
    P1760 Control Module - Programming
    P1761 Control Module - KAM
    P1762 Control Module - RAM
    P1763 Control Module - ROM
    P1764 Can Controller - Malfunction
    P1764 ECU-ITM CAN Communication Line
    P1765 Torque Reduction Request
    P1765 Torque Reduction Request
    P1765 TCS-ITM CAN Communication Line
    P1766 Torque Reduction Execution
    P1766 Torque Reduction Execution
    P1771 Shifting Position
    P1772 Selection Position
    P1773 Gearbox Input Speed Sensor
    P1774 CRC Sensor
    P1775 TCM-Malfunction
    P1776 Power Control Module- Malfunction
    P1777 PCM Output - Malfunction
    P1778 Electro Valve Output - Malfunction
    P1779 Stator Inhibit Relay - Malfunction
    P1780 Push-Pull Sensor
    P1780 Torque Reduction Required Signal
    P1781 Actuator
    P1782 Actuator Position Sensor
    P1786 Engine Rpm Output Circuit Malfunction
    P1791 TPS Input Circuit - Malfunction
    P1795 Ground Return Circuit - Malfunction
    P1795 Transfer High/Low Switch - Malfunction
    P1800 Immobilizer Antenna Error
    P1801 Immobilizer Transponder Error
    P1802 Immobilizer Antenna Error
    P1803 Immobilizer ECU Signal Error
    P1805 Immobilizer EEPROM Error
    P1805 EEPROM Error/VIN Mismatch
    P2096 Post Fuel Trim Too Lean-B1
    P2097 Post Fuel Trim Too Rich-B1
    P2120 Accelerator Position Circuit - Malfunction
    P2122 Accelerator Position Circuit - Low Input
    P2123 Accelerator Position Circuit - High Input
    P2125 Accelerator Position Circuit - Malfunction
    P2127 Accelerator Position Circuit - Low Input
    P2128 Accelerator Position Circuit - High Input
    P2187 Fuel System Too Lean At Idle
    P2188 Fuel Sys Too Rich At Idle
    P2191 Fuel Sys Too Lean At P-load
    P2192 Fuel Sys Too Rich At P-load
    P2195 L.O2s Signal Stuck - Lean (B1/S1)
    P2196 L.O2s Signal Stuck - Rich (B1/S1)
    P2231 O2s Signal & Heater - Short (B1/S1)
    P2237 L.O2s Pump. Circuit - Open (B1/S1)
    P2238 L.O2s Pump. Circuit - Low (B1/S1)
    P2239 L.O2s Pump. Circuit - High (B1/S1)
    P2243 L.O2s Reference Circuit (+) Open - (B1/S1)
    P2251 L.O2s Reference Circuit (-) Open - (B1/S1)
    P2297 L.O2s Out Of Range (B1/S1)
    P2414 L.O2s Exhaust Sample (B1/S1)
    P2626 L.O2s Pump Trim - Open (B1/S1)
</details>

## Arduino example code

https://cafe.naver.com/ittec/104
[출처] OBD-II PID 송수신 함수 (통신쟁이) | 작성자 테리우스

SAE J1979의 PID 송수신 처리
참조 사이트 : http://opengauge.googlecode.com/svn/trunk/obduino/

이 자료는 OBD-II 인터페이스 중에서, K Line Interface MC33290을 사용하는 진단기에서,
ISO 9141 표준으로 K-Line 인터페이스를 초기화, 송신, 수신하는 함수에 대한 설명이다.

ISO 9141의 연결 방법 (적색은 ECU의 연결 응답)
1(300ms) -> 33(1060ms) -> 55(0.96) -> 08(0.96) -> 08(0.96) -> 7F(0.96) -> CC(0.96) [ms]
대기 연결요청(5 bps) 응답1 응답2 응답3 연결OK 응답4
```arduino
// 포트를 정의
#define K_IN 2
#define K_OUT 3
#define TOPLEFT 0
#define TOPRIGHT 1
#define BOTTOMLEFT 2
#define BOTTOMRIGHT 3

// LCD 핀의 정의
이 값을 바꾸면, 아듀이노 기판에 연결하는 LCD의 포트를 바꿀 수 있다.
#define DIPin 4 // LCD RS 핀
#define EnablePin 5 // lCD ENB 핀

#define DB4Pin 7
#define DB5Pin 8
#define DB6Pin 12
#define DB7Pin 13

#define ContrastPin 6 // LCD의 명암을 누름단추로 조절
#define BrightnessPin 9 // LCD의 밝기를 누름단추로 조절

// 포트를 설정
pinMode(K_OUT, OUTPUT); // ISO 9141 TX 핀
pinMode(K_IN, INPUT); // ISO 9141 RX 핀

pinMode( lbuttonPin, INPUT ); // 누름 단추 3 개는 입력
pinMode( mbuttonPin, INPUT );
pinMode( rbuttonPin, INPUT );

digitalWrite( lbuttonPin, HIGH); // 누름단추 포트용 내부 풀업 (포트핀의 풀업저항이 필요없다)
digitalWrite( mbuttonPin, HIGH);
digitalWrite( rbuttonPin, HIGH);
```
-----------------------------------------------------------------------------------------------------------------
파라메터, LCD를 초기화
```arduino
if(params_load() ==0) // 설정값이 없으면, 다음과 같이 초기값을 저장
{
  params.contrast =40;
  params.useMetric =1;
  params.perHourSpeed =20;
  params.vol_eff =80; // 연료 효율 =80%, MPA을 위한 근접에 필요
  params.eng_dis =20; // 엔진 크기 =2.0L
  params.trip_dist =0.0;
  params.trip_fuel =0.0;
  params.screen[0].corner[TOPLEFT] =FUEL_CONS;
  params.screen[0].corner[TOPRIGHT] =TRIP_CONS;
  params.screen[0].corner[BOTTOMLEFT] =ENGINE_RPM;
  params.screen[0].corner[BOTTOMRIGHT] =VEHICLE_SPEED;
  params.screen[1].corner[TOPLEFT] =TRIP_CONS;
  params.screen[1].corner[TOPRIGHT] =TRIP_DIST;
  params.screen[1].corner[BOTTOMLEFT] =COOLANT_TEMP;
  params.screen[1].corner[BOTTOMRIGHT] =CAT_TEMP_B1S1;
  params.screen[2].corner[TOPLEFT] =PID_SUPPORT20;
  params.screen[2].corner[TOPRIGHT] =PID_SUPPORT40;
  params.screen[2].corner[BOTTOMLEFT] =PID_SUPPORT60;
  params.screen[2].corner[BOTTOMRIGHT] =OBD_STD;
}
```
-----------------------------------------------------------------------------------------------------------------
ISO 9141 포트를 초기화
ISO 9141의 인터페이스 방법은 PID를 송수신하는 방법으로 설명됩니다.
```arduino
byte iso_init()
{
  byte b;

  digitalWrite(K_OUT, HIGH); // K핀을 300ms 동안 HIGh로 만듭니다, (ECU 대기)
  delay(300);

  digitalWrite(K_OUT, LOW); // K핀을 200ms 동안 LOW로 만듭니다, 5bps 시작비트
  delay(200);
  b =0x33; // 0x33 즉 00110011를 송신, 5 bps 데이터 비트 8개
  for (byte mask = 0x01; mask; mask <<= 1) // mask값 0x01을 0x80 이 될 때 까지 8번 반복 (LSB 우선송신)
  {
  if (b & mask) digitalWrite(K_OUT, HIGH); // 비트 1을 송신
  else digitalWrite(K_OUT, LOW); // 비트 0을 송신
  delay(200); // 비트의 간격은 5 bps로 200ms 이다 (아직은 ISO 9141 송신함수를 사용할 수 없다)
  }

  digitalWrite(K_OUT, HIGH); // 정지 비트의 송신, 200ms 이 필요,
  delay(200); // 5 bps 정지 비트
  delay(60); // 60ms 추가로 대기한다 (ISO 표준 값이다) 이제 ECU와 ISO9141 통신이 가능하다.

  b =iso_read_byte(); // 이제 10400 pcs로 ECU의 데이터를 기다린다
  if (b !=0x55) return -1; // ECU에서 응답하는 첫 데이터가 0x55 가 아니면 연결 실패다.
  delay(5);

  b=iso_read_byte(); // 첫 데이터가 0x55이면, ECU의 다음 데이터를 기다린다.
  if (b !=0x08) return -1; // 두번째 데이터가 0x08이 아니면 연결 실패이다.
  delay(20);

  b =iso_read_byte(); // 두번째 데이터가 0x08 이면, ECU의 다음 데이터를 기다린다.
  if(b!=0x08) return -1; // 세번째 데이터가 0x08 이면, 연결이 성공, ECU로 응답을 해야 한다
  delay(25);

  iso_write_byte(0xF7); // ECU의 응답이 성공이라는 확인으로 0xF7을 송신한다
  delay(25);

  b =iso_read_byte(); // ECU로 부터 마지막 응답 0xCC가 들어 오면 연결괸 것이다. (연결 성공)
  if(b !=0xCC) return -1; // ISO 9141 연결이 실패이면 -1을 가지고 나간다
  return 0; // ISO 9141 연결이 성공이면 0을 가지고 나간다
}
```
이제 ISO 9141 연결에 성공하였으므로, ISO 9141 송/수신 함수를 사용할 수 있다.

-----------------------------------------------------------------------------------------------------------------
ISO 9141 송신 함수
```arduino
#define _bitPeriod 1000000L/10400L // ISO 9141의 1 비트 주기는 10400 bps = 1000000/10400 = 96

void iso_write_byte(byte b)
{
  int bitDelay = _bitPeriod - clockCyclesToMicroseconds(50); // 비트주기 -digitalWrite의 처리시간을 보정
  digitalWrite(K_OUT, LOW);
  delayMicroseconds(bitDelay); // ISO 9141 연결핀 K에 LOW를 1비트 송신한다 (시작 비트)

  for (byte mask = 0x01; mask; mask <<= 1) // LSB를 우선으로 8 비트를 송신 (무조건 송신)
  {
  if (b & mask) digitalWrite(K_OUT, HIGH); // 비트 1을 1개 송신
  else digitalWrite(K_OUT, LOW); // 비트 0을 1개 송신
  delayMicroseconds(bitDelay);
  }
  digitalWrite(K_OUT, HIGH); // ISO 9141에 연결된 핀 K에 HIGH를 송신 (정지 비트)
  delayMicroseconds(bitDelay); // ISO 9141는 1선으로 송수신하므로 송신할 때는 수신이 안된다.
}
```
-----------------------------------------------------------------------------------------------------------------
ISO 9141 수신함수
```arduino
int iso_read_byte()
{
  int val = 0;
  unsigned long timeout;
  int bitDelay = _bitPeriod - clockCyclesToMicroseconds(50); // 비트주기 -digitalWrite의 처리시간을 보정

  timeout =millis();
  while (digitalRead(K_IN)) // 시작 비트를 기다린다
  {
    if((millis()-timeout) > 300L) return -1; // 300ms이 지나면 수신 불량이다.
  }

  if (digitalRead(K_IN) == LOW) // 처음으로 LOW가 나타나면, 이제부터 수신이 시작된다
  {
    delayMicroseconds(bitDelay / 2 - clockCyclesToMicroseconds(50)); // 비트폭의 중간에서 수신을 시작한다.
    for (int offset =0; offset <8; offset++) // 8 비트를 수신한다, 비트0 ~ 비트7을 수신
    {
      delayMicroseconds(bitDelay); // 비트폭의 중간에서 비트값을 읽는다
      val |=digitalRead(K_IN) << offset; // 읽은 비트를 변수에 저장하고, 좌로 1비트 시프트
    }
    delayMicroseconds(_bitPeriod); // 남은 비트폭을 기다린다 (다음 바이트 수신을 위함)
    return val; // 수신한 8 비트 =1 바이트를 가지고 나간다. (수신 성공)
  }
  return -1; // 수신 실패는 -1을 가지고 나간다
}
```
-----------------------------------------------------------------------------------------------------------------
ISO 9141 테이터 읽기
ECU에서 응답하는 ISO 9141의 데이터 패킷을 iso_read_byte()을 호출하여 읽는다
ECU의 응답 데이터 패킷은 +header + cmd + crc로 구성된다.
수신할 바이트 갯수와 저장할 버퍼를 가지고 호출해야 한다, 데이터만 buf[20]에 저장된다.
```arduino
  byte iso_read_data(byte *data, byte len)
  {
    byte i;
    byte buf[20];

    // 머리문자 3 바이트: [80 + datalen] [destination = F1] [source = 01] (예: 83, F1, 01)
    // 데이터 1 + len 바이트 : [40 +cmd0] [result0], CRC, (예: C1, E9, 8F, AE)
    for(i=0; i<3+1+1+len; i++) // 수신할 바이트의 갯수는 5 + 데이터 길이 len 이다.
    buf[i] =iso_read_byte(); // ISO 9141 에서 데이터를 바이트로 읽는다
    // 머리문자 3개는 점검하지 않는다, 오류코드 0x7f와 CRC도 점검하지 않는다.
    memcpy(data, buf +4, len); // 수신한 패킷에서, 데이터는 5번째 바이트 부터이다.
    return len;
  }
```
진단기가 ECU로 PID를 송신하면. (예 MODE=01,PID=00의 송신은 0x68 0x6A 0xF1 0x01 0x00 0xC4)
ECU가 진단기로 응답한다. (예 응답은 0x48 0x6B 0x41 0x01 XX XX XX XX CRC 이다, XX는 응답 데이터)
송신 머리문자 0x68 0x6A 0xF1는 진단기에서 PID를 송신할 때 추가되며,
수신 머리문자 0x48 0x6B 0x41 0x01는 ECU에서 응답할 때 추가된다. (0x41은 ECU 번호+0x40, 0x01은 요청한 PID)

-----------------------------------------------------------------------------------------------------------------
ISO 9141 테이터 쓰기
송신버퍼 data에 저장된 데이터에 송신용 머리문자 3개를 추가하고,
송신 데이터의 마지막에는 CRC를 만들어 추가한 다음에.
송신버퍼에 들아있는 모든 문자를 iso_write_byte() 함수를 반복 호출하여 송신한다.
```arduino
byte iso_write_data(byte *data, byte len)
{
byte i, n;
byte buf[20];

buf[0] =0x68; // ISO 머리문자(header)
buf[1] =0x6A; // 0x68 0x6A 는 OBD-II 의 요청 명령이다t
buf[2] =0xF1; // 진단기의 주소 (송신주소)

for (i =0; i<len; i++) buf[i+3] =data[i]; // 머리문자는 3개 뒤에 데이터를 추가한다
i +=3;
buf[i] =iso_checksum(buf, i); // 송신 CRC를 만든다음 송신버퍼의 마지막에 추가한다
n =i +1; // 송신 바이트 갯수에 CRC를 추가한다
for(i =0; i <n; i++)
{
iso_write_byte(buf[i]); // 1 바이트씩 송신버퍼를 ISO 9141 인터페이스로 송신한다
delay(20); // 1 바이트 마다 20ms 지연시간을 준다.
}
return 0;
}
```
-----------------------------------------------------------------------------------------------------------------
SAE J1979의 CRC생성 함수
J1979의 CRC는 머리,,, 데이터끝 까지 0x00에 모두 더한 값이며, 덧셈에서 발행하는 자리올림은 사용하지 않고 버린다.
J1979의 CRC는 송,수신 패킷의 마지막에 추가 되어야 하며, 주로 송신 패킷에서 사용한다.
```arduino
byte iso_checksum(byte *data, byte len)
{
byte i;
byte crc;

crc=0; // CRC의 초기값은 0 이다.
for(i=0; i <len; i++) crc=crc +data[i]; // 송신버퍼에 들은 데이터를 모두 CRC에 더한다
return crc;
}
```
-----------------------------------------------------------------------------------------------------------------
SAE J1979의 PID 읽기 함수
참조 사이트 : http://opengauge.googlecode.com/svn/trunk/obduino/

이 자료는 OBD-II 인터페이스 중에서, K Line Interface MC33290을 사용하는 진단기에서,
SAE J1979 표준 PID를 송신하고, 응답하는 데이터를 수신하는 PID 함수이다.
이 함수를 실행하려면, ECU로 요청하는 PID 번호 와 ECU의 응답을 저장하는 32비트 버퍼가 필요하다.
PID 번호는 ISO송신 함수를 실행하여 ECU로 전송되고, ISO수신 함수에서 받은 데이터는 지정된 retbuf에 저장한다.
```arduino
unsigned long pid01to20_support;
unsigned long pid21to40_support;
unsigned long pid41to60_support;

long get_pid(byte pid, char *retbuf)
{
byte i;
byte cmd[2]; // 송신 PID를 저장하는 변수
byte buf[10]; // 수신 데이터를 저장하는 변수
long ret; // 결과를 저장하는 4바이트 변수
byte reslen; // 결과의 길이를 저장하는 변수
char decs[16]; // 결과를 문자로 변환하는 작업용 변수

if( pid !=0x00) // 0x00 =PID_SUPPORT20 , PID 요청값이 PID를 지원하는지 확인한다.
{
if( (pid <=0x20 && ( 1L <<(0x20 -pid) & pid01to20_support ) == 0 )
|| (pid >0x20 && pid <=0x40 && ( 1L <<(0x40-pid) & pid21to40_support ) == 0 )
|| (pid >0x40 && pid <=0x60 && ( 1L <<(0x60-pid) & pid41to60_support ) == 0 )
|| (pid >LAST_PID) )
{
sprintf_P(retbuf, PSTR("%02X N/A"), pid); // PID 확인결과를 "아님"으로 표시
return -1; // 아닌 경우는 나머지를 실해하지 않고 이 함수를 끝낸다.
}
}
reslen =pgm_read_byte_near(pid_reslen+pid); // PID 번호에 따르는 데이터의 길이를 구한다
cmd[0] =0x01; // ISO 명령은 1 바이트이다
cmd[1] =pid; // 요청된 PID값을 변수에 저장
iso_write_data(cmd, 2); // ISO 송신함수를 실행, PID값을 ECU로 송신.
iso_read_data(buf, reslen); // ISO 수신함수를 실행, ECU에서 응답한 데이터를 수신.

switch(pid) // SAE 표준 공식과 단위를 사용, PID 응답을 분리하고 계산한다.
{
case 0x0C: // ENGINE_RPM =엔진 회전수
ret=(buf[0]*256U+buf[1])/4U;
sprintf_P(retbuf, PSTR("%ld RPM"), ret);
break;

case 0x10: // MAF_AIR_FLOW =흡기관 공기속도
ret=buf[0]*256U+buf[1];
int_to_dec_str(ret, decs, 2); // not divided by 100 for return value!!
sprintf_P(retbuf, PSTR("%s g/s"), decs);
break;

case 0x0D: // VEHICLE_SPEED =차량 속도
ret=buf[0];
if(!params.useMetric) ret=(ret*621U)/1000U;
sprintf_P(retbuf, PSTR("%ld %s"), ret, params.useMetric?" ":" ");
break;

case 0x03: // FUEL_STATUS =연료 상태
ret=buf[0] *256U +buf[1];
if(buf[0]==0x01) sprintf_P(retbuf, PSTR("OPENLOWT")); // 엔진 온도 낮음
else if(buf[0]==0x02) sprintf_P(retbuf, PSTR("CLSEOXYS")); // Closed loop, 산소 센서값을 연료혼합으로 되돌린다.
else if(buf[0]==0x04) sprintf_P(retbuf, PSTR("OPENLOAD")); // Open loop 엔진 부하 [%]
else if(buf[0]==0x08) sprintf_P(retbuf, PSTR("OPENFAIL")); // Open loop 고장난 장치
else if(buf[0]==0x10) sprintf_P(retbuf, PSTR("CLSEBADF")); // Closed loop, 최근에 고장난 장치의 산소센서
else sprintf_P(retbuf, PSTR("%04lX"), ret);
break;

// 다음 12개의 PID 결과는 1 바이트로 모두 [%] 형식이다.
case 0x04: // LOAD_VALUE =엔진 부하
case 0x11: // THROTTLE_POS =가속발판 위치
case 0x45: // REL_THR_POS =가속발판 위치 %값
case 0x2C: // EGR =배기 혼합장치
case 0x2D: // EGR_ERROR =배기 혼합장치 오류
case 0x2F: // FUEL_LEVEL =연료 잔량
case 0x47: // ABS_THR_POS_B =미끄럼방지 가속기 위치 B
case 0x48: // ABS_THR_POS_C =미끄럼방지 가속기 위치 C
case 0x49: // ACCEL_PEDAL_D =가속 D
case 0x4A: // ACCEL_PEDAL_E =가속 E
case 0x4B: // ACCEL_PEDAL_F =가속 F
case 0x4C: // CMD_THR_ACTU =가속 구동값:
ret=(buf[0] *100U) /255U; // 0~255 범위의 결과를 [%]로 계산한다.
sprintf_P(retbuf, PSTR("%ld %%"), ret); // %값을 10진으로 변환하고, ret 변수에 저장

// 다음 8개의 PID 응답은 [mV]로 변환한다.
case 0x14: // B1S1_O2_V =산소센서 1-1 전압
case 0x15: // B1S2_O2_V =산소센서 1-2 전압
case 0x16: // B1S3_O2_V =산소센서 1-3 전압
case 0x17: // B1S4_O2_V =산소센서 1-4 전압
case 0x18: // B2S1_O2_V =산소센서 2-1 전압
case 0x19: // B2S2_O2_V =산소센서 2-2 전압
case 0x1A: // B2S3_O2_V =산소센서 2-3 전압
case 0x1B: // B2S4_O2_V =산소센서 2-4 전압
ret =buf[0] *5U; // 결과를 1000으로 나누지 않음
if(buf[1] ==0xFF) sprintf_P(retbuf, PSTR("%ld mV"), ret); // 첫 바이트 = 0xFF 이면, 결과를 바로 10진으로 저장
else sprintf_P(retbuf, PSTR("%ldmV/%d%%"), ret, ((buf[1]-128)*100)/128); // 0xFF가 아니면, 결과를 계산하고 저장
break; // 결과의 첫 바이트 최상위 비트가 1이 아니면, 나머지 7비트를 [%]로 계산한다.

case 0x21: // DIST_MIL_ON =계기판의 엔진고장 등을 켠다
case 0x31: // DIST_MIL_CLR =계기판의 엔진고장 등을 끈다
ret =buf[0] *256U +buf[1]; // 결과 1 바이트 2 개를, 2바이트 정수로 변환한다.
if(!params.useMetric) ret =(ret *621U) /1000U; // 저장된 2바이트 정수를 *621/1000으로 계산한다
sprintf_P(retbuf, PSTR("%ld %s"), ret, params.useMetric?" ":"mi"); // 10진으로 변환해서 문자로 저장한다
break;

case 0x4D: // TIME_MIL_ON =고장표시등이 점등된 시간
case 0x4E: // TIME_MIL_CLR =고장 표시등이 소등된 시간
ret=buf[0] *256U +buf[1]; // 2 바이트 정수로 변환
sprintf_P(retbuf, PSTR("%ld min"), ret); // 10진 문자로 변환한 시간을 분(min)으로 표시한다
break;

case 0x05: // COOLANT_TEMP =냉각수 온도
case 0x0F: // INT_AIR_TEMP =흡기관 온도
case 0x46: // AMBIENT_TEMP =주위 온도
case 0x3C: // CAT_TEMP_B1S1 =환원장치 온도 #1-1
case 0x3D: // CAT_TEMP_B2S1 =환원장치 온도 #2-1
case 0x3E: // CAT_TEMP_B1S2 =환원장치 온도 #1-2
case 0x3F: // CAT_TEMP_B2S2 =환원장치 온도 #2-2
if(pid >=0x3C && pid <=0x3F) // 산소센서의 온도를 계산
ret =(buf[0] *256U +buf[1]) /10U -40; // 10으로 나누고 40을 뺀다음, 온도로 저장한다 .
else ret =buf[0] -40; // 산소센서가 아니면 40을 뺀다음, 온도로 저장한다.

if(!params.useMetric) ret =(ret *9) /5 +32; // 선택한 단위가 Metric 이면 섭씨로 변환한다
sprintf_P(retbuf, PSTR("%ld %c"), ret, params.useMetric?'C':'F'); // 10진 문자로 단위와 같이 저장한다,
break;

case 0x06: // STF_BANK1 =단시간 연료 %조절 #1
case 0x07: // LTR_BANK1 =장시간 연료 %조절 #1
case 0x08: // STF_BANK2 =단시간 연료 %조절 #2
case 0x09: // LTR_BANK2 =장시간 연료 %조절 #2
ret=(buf[0]-128) *7812; // 첫 바이트 최상위 비트를 지우고, 7 비트(A6~A0)에 7812를 곱한다음 저장한다.
int_to_dec_str(ret/100, decs, 2); // 결과를 100으로 나누고, 10진 문자 2자리로 변환한다, (10000 으로 나누지 않음)
sprintf_P(retbuf, PSTR("%s %%"), decs);
break;

case 0x0A: // FUEL_PRESSURE =연료분사 압력
case 0x0B: // MAN_PRESSURE =분기관 압력
case 0x33: // BARO_PRESSURE =공기 압력
ret=buf[0];
if(pid ==0x0A) ret *=3U; // 만일 응답값이 FUEL_PRESSURE 이면, 3을 곱해서 저장한다.
sprintf_P(retbuf, PSTR("%ld kPa"), ret); // 표시하는 값은 kPa(압력의 단위) 이다
break;

case 0x0E: // TIMING_ADV =실린더 상대 각도 [deg] (점화시기)
ret =(buf[0] /2) -64; // 응답값을 2로 나누고 -64를 한다음 저장한다.
sprintf_P(retbuf, PSTR("%ld "), ret);
break;

case 0x1C: // OBD_STD =연결된 ECU가 지원하는 OBD 표준을 나타낸다 (254 바이트)
ret =buf[0]; // 응답으로 들어온 값으로 부터, OBD 표준에 해당하는 하나의 이름을 선택하여 표시한다
if(buf[0] ==0x01) sprintf_P(retbuf, PSTR("OBD2CARB")); // 응답이 0x01 이면 OBD2 CARB 표준을 지원한다.
else if(buf[0]==0x02) sprintf_P(retbuf, PSTR("OBD2EPA")); // 응답이 0x02 이면 OBD2 EPA 표준을 지원한다.
else if(buf[0]==0x03) sprintf_P(retbuf, PSTR("OBD1&2")); // 응답이 0x03 이면 OBD1과 2 표준을 지원한다.
else if(buf[0]==0x04) sprintf_P(retbuf, PSTR("OBD1")); // 응답이 0x04 이면 OBD1 표준을 지원한다.
else if(buf[0]==0x05) sprintf_P(retbuf, PSTR("NOT OBD")); // 응답이 0x05 이면 OBD를 지원하지 않는다.
else if(buf[0]==0x06) sprintf_P(retbuf, PSTR("EOBD")); //응답이 0x06 이면 EOBD 표준을 지원한다.
else if(buf[0]==0x07) sprintf_P(retbuf, PSTR("EOBD&2")); //응답이 0x07 이면 EOBD2 표준을 지원한다.
else if(buf[0]==0x08) sprintf_P(retbuf, PSTR("EOBD&1"));//응답이 0x08 이면 EOBD1 표준을 지원한다.
else if(buf[0]==0x09) sprintf_P(retbuf, PSTR("EOBD&1&2"));//응답이 0x09 이면 EOBD1과 2 표준을 지원한다.
else if(buf[0]==0x0a) sprintf_P(retbuf, PSTR("JOBD")); //응답이 0x0A 이면 JOBD 표준을 지원한다.
else if(buf[0]==0x0b) sprintf_P(retbuf, PSTR("JOBD&2")); //응답이 0x0B 이면 JOBD2 표준을 지원한다.
else if(buf[0]==0x0c) sprintf_P(retbuf, PSTR("JOBD&1")); //응답이 0x0C 이면 JOBD1 표준을 지원한다.
else if(buf[0]==0x0d) sprintf_P(retbuf, PSTR("JOBD&1&2")); // 응답이 0x0D이면, JOBD1과 2 표준을 지원한다.
else sprintf_P(retbuf, PSTR("OBD:%02X"), buf[0]); // 위 항목에서 발견하지 못한 값이면, OBD ?? 표준을 지원한다.
break;

default: // 처리항목에 없는 데이터는 들어온 값을 처리없이 그대로 표시한다
ret=0; // (처리하지 못한 항목은, 나가는 값이 0 이다)
for(i=0; i<reslen; i++)
{
ret *=256L;
ret +=buf[i]; // 응답 1 바이트 2개를, 2 바이트 정수로 변환
}
sprintf_P(retbuf, PSTR("%08lX"), ret); // 문자로 변환하여 retbuf에 저장
break;
}
return ret; // 선택 처리된 1개 항목의 결과는 ret에 저장된다, 2 바이트 정수값을 주함수로 돌려준다
} // 수신된 데이터를 LCD에 표시하는 문자는 retbuf 에 저장되어 있다.
```

