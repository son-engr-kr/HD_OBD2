# 1 "C:\\HD_OBD2\\HD_OBD2\\Arduino_project\\obd2_project\\obd2_project.ino"
// Service 01 PIDs (more detail: https://en.wikipedia.org/wiki/OBD-II_PIDs)
// 설정 참고 https://ttuk-ttak.tistory.com/31
# 55 "C:\\HD_OBD2\\HD_OBD2\\Arduino_project\\obd2_project\\obd2_project.ino"
//----------------------------------------------



# 60 "C:\\HD_OBD2\\HD_OBD2\\Arduino_project\\obd2_project\\obd2_project.ino" 2
# 61 "C:\\HD_OBD2\\HD_OBD2\\Arduino_project\\obd2_project\\obd2_project.ino" 2


MCP_CAN CAN0(9); // Set CS to pin 10 <--------- CHANGE if using different pin number

long unsigned int rxId;
unsigned char len = 0;
unsigned char rxBuf[8];
char msgString[128]; // Array to store serial string
void SendStoredDTC()
{
  unsigned char tmp[8] = {0x02, 0x03, 0x01, 0, 0, 0, 0, 0};

   byte sndStat = CAN0.sendMsgBuf(0x7DF /*OBD-II CAN frame ID*/, 0, 8, tmp);

  if (sndStat == (0)) {
    Serial.println("Request stored DTC");
  }
  else {
    Serial.println("Error Sending Message...");
  }

}
void ReceiveStoredDTC()
{
  if (!digitalRead(2 /* Set INT to pin 2  <--------- CHANGE if using different pin number*/)) { // If CAN0_INT pin is low, read receive buffer
    CAN0.readMsgBuf(&rxId, &len, rxBuf); // Read data: len = data length, buf = data byte(s)

    sprintf(msgString, "Standard ID: 0x%.3lX, DLC: %1d, Data: ", rxId, len);
    Serial.print(msgString);

    for (byte i = 0; i < len; i++) {
      sprintf(msgString, " 0x%.2X", rxBuf[i]);
      Serial.print(msgString);
    }
    Serial.println("");

    switch (rxBuf[3]>>6& 0x3){
      uint2_t secondCode = (rxBuf[3]>>4) & 0x3;
      uint4_t thirdCode = (rxBuf[3]) & 0xF;
      uint4_t fourthCode = (rxBuf[4]>>4) & 0xF;
      uint4_t fifthCode = (rxBuf[4]) & 0xF;
      case 0x00:
        Serial.print("DTC code is : P");
        Serial.print(secondCode, 10); //DTC에 각 자리수마다 9를 넘어가는 수가 없어서 10진법으로 해도 상관없을듯.
        Serial.print(thirdCode, 10);
        Serial.print(fourthCode, 10);
        Serial.println(fifthCode, 10);
        break;
      case 0x01:
        Serial.print("DTC code is : C");
        Serial.print(secondCode, 10);
        Serial.print(thirdCode, 10);
        Serial.print(fourthCode, 10);
        Serial.println(fifthCode, 10);
        break;
      case 0x02:
        Serial.print("DTC code is : B");
        Serial.print(secondCode, 10);
        Serial.print(thirdCode, 10);
        Serial.print(fourthCode, 10);
        Serial.println(fifthCode, 10);
        break;
      case 0x03:
        Serial.print("DTC code is : U");
        Serial.print(secondCode, 10);
        Serial.print(thirdCode, 10);
        Serial.print(fourthCode, 10);
        Serial.println(fifthCode, 10);
        break;

// 진단 코드가 두개일때 
    switch (rxBuf[5]>>6& 0x3){
      uint2_t secondCode = (rxBuf[5]>>4) & 0x3;
      uint4_t thirdCode = (rxBuf[5]) & 0xF;
      uint4_t fourthCode = (rxBuf[6]>>4) & 0xF;
      uint4_t fifthCode = (rxBuf[6]) & 0xF;
      case 0x00:
        Serial.print("DTC code is : P");
        Serial.print(secondCode, 10);
        Serial.print(thirdCode, 10);
        Serial.print(fourthCode, 10);
        Serial.println(fifthCode, 10);
        break;
      case 0x01:
        Serial.print("DTC code is : C");
        Serial.print(secondCode, 10);
        Serial.print(thirdCode, 10);
        Serial.print(fourthCode, 10);
        Serial.println(fifthCode, 10);
        break;
      case 0x02:
        Serial.print("DTC code is : B");
        Serial.print(secondCode, 10);
        Serial.print(thirdCode, 10);
        Serial.print(fourthCode, 10);
        Serial.println(fifthCode, 10);
        break;
      case 0x03:
        Serial.print("DTC code is : U");
        Serial.print(secondCode, 10);
        Serial.print(thirdCode, 10);
        Serial.print(fourthCode, 10);
        Serial.println(fifthCode, 10);
        break;

    }

}
void sendPID(unsigned char __pid)
{
  unsigned char tmp[8] = {0x02, 0x01, __pid, 0, 0, 0, 0, 0};

  byte sndStat = CAN0.sendMsgBuf(0x7DF /*OBD-II CAN frame ID*/, 0, 8, tmp);

  if (sndStat == (0)) {
    Serial.print("PID sent: 0x");
    Serial.println(__pid, 16);
  }
  else {
    Serial.println("Error Sending Message...");
  }
}

void receivePID(unsigned char __pid)
{
    if (!digitalRead(2 /* Set INT to pin 2  <--------- CHANGE if using different pin number*/)) { // If CAN0_INT pin is low, read receive buffer
    CAN0.readMsgBuf(&rxId, &len, rxBuf); // Read data: len = data length, buf = data byte(s)

    sprintf(msgString, "Standard ID: 0x%.3lX, DLC: %1d, Data: ", rxId, len);
    Serial.print(msgString);

    for (byte i = 0; i < len; i++) {
      sprintf(msgString, " 0x%.2X", rxBuf[i]);
      Serial.print(msgString);
    }
    Serial.println("");


    switch (__pid) {
      case 0x05:
        if(rxBuf[2] == 0x05){
          uint8_t temp;
          temp = rxBuf[3] - 40;
          Serial.print("Engine Coolant Temp (degC): ");
          Serial.println(temp, 10);
        }
      break;

      case 0x0C:
        if(rxBuf[2] == 0x0C){
          uint16_t rpm;
          rpm = ((256 * rxBuf[3]) + rxBuf[4]) / 4;
          Serial.print("Engine Speed (rpm): ");
          Serial.println(rpm, 10);
        }
      break;
    }
  }
}

void setup()
{
  Serial.begin(115200);

  // Initialize MCP2515 running at 16MHz with a baudrate of 500kb/s and the masks and filters disabled.
  if (CAN0.begin(0 /* Standard and Extended        */, 13, 2) == (0)) { //< -------- - CHANGE if using different board
    Serial.println("MCP2515 Initialized Successfully!");
  }
  else {
    Serial.println("Error Initializing MCP2515...");
    while (1);
  }

  //initialise mask and filter to allow only receipt of 0x7xx CAN IDs
  CAN0.init_Mask(0, 0, 0x07000000); // Init first mask...
  CAN0.init_Mask(1, 0, 0x07000000); // Init second mask...


  for (uint8_t i = 0; i < 6; ++i) {
    CAN0.init_Filt(i, 0, 0x07000000); //Init filters
  }

  CAN0.setMode(0x00); // Set operation mode to normal so the MCP2515 sends acks to received data.

  pinMode(2 /* Set INT to pin 2  <--------- CHANGE if using different pin number*/, 0x0); // Configuring pin for /INT input

  Serial.println("Sending and Receiving OBD-II_PIDs Example...");
}

void loop()
{
  //request coolant temp
  sendPID(0x05);

  delay(40); //to allow time for ECU to reply

  receivePID(0x05);

  //request engine speed
  sendPID (0x0C);

  delay(40); //to allow time for ECU to reply

  receivePID(0x0C);

  //abitrary loop delay
  delay(40);

  delay(500);

}
