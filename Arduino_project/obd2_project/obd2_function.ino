
#include <mcp_can.h>
#include <SPI.h>

MCP_CAN CAN0(9);                               // Set CS to pin 10 <--------- CHANGE if using different pin number

long unsigned int rxId;
unsigned char len = 0;

char msgString[128];                        // Array to store serial string
char buffer[300];
char DTCFirstCode;

int requestCount = 0;
unsigned char pidLowSignificant[] = {
    PID_COOLANT_TEMP,
    PID_ENGINE_FUEL_RATE,
    PID_FUEL_LEVEL
  };


void PrintOBD2Data(char* HEADER, char* CATEGORY, char* pidname, double value){
  sprintf(buffer,"%s,%s,%s,%d",HEADER,CATEGORY,pidname,value);
  Serial.println(buffer);
}

void PrintOBD2DTC(char* HEADER, char* CATEGORY,char char1, int num2, int num3, int num4, int num5 , double value){
  sprintf(buffer,"%s,%s,%c%d%d%d%d,%d",HEADER,CATEGORY,
  char1,num2,num3,num4,num5,value );
  Serial.println(buffer);
}

void ClearDTCandMIL(){
  unsigned char tmp[8] = {0x01, 0x04, 0x00, 0, 0, 0, 0, 0};

  byte sndStat = CAN0.sendMsgBuf(CAN_ID_PID, 0, 8, tmp);

  if (sndStat == CAN_OK) {
    PrintOBD2Data(OBD2_HEADER,OBD2_CATEGORY_DTC,"Clear_Test_Sending_Success",0);
    // sprintf(buffer,"%s,%s,%s,%d",OBD2_HEADER,OBD2_CATEGORY_DTC,"Clear_Test_Sending_Success",0 );
    // Serial.println(buffer);
    // Serial.println("OBD2____,DTC,Clear_Test_Sending_Success,0");
  }
  else {
    Serial.println("Error Sending Message...");
    PrintOBD2Data(OBD2_HEADER,OBD2_CATEGORY_DTC,"Clear_Test_Sending_fail",0);
    // Serial.println("OBD2____,DTC,Clear_Test_Sending_fail,0");
    
    // sprintf(buffer,"%s,%s,%s,%d",OBD2_HEADER,OBD2_CATEGORY_DTC,"Clear_Test_Sending_fail",0 );
    // Serial.println(buffer);
  }

}


void requestStoredDTC()
{
  unsigned char tmp[8] = {0x01, 0x03, 0x00, 0, 0, 0, 0, 0};

   byte sndStat = CAN0.sendMsgBuf(CAN_ID_PID, 0, 8, tmp);

  if (sndStat == CAN_OK) {
    Serial.println("Request stored DTC");
    PrintOBD2Data(OBD2_HEADER,OBD2_CATEGORY_DTC,"Request_Test_Sending_Success",0);
  }
  else {
    Serial.println("Error Sending Message...");
    PrintOBD2Data(OBD2_HEADER,OBD2_CATEGORY_DTC,"Request_Test_Sending_fail",0);
  }

}
void receiveOBD(){
  unsigned char rxBuf[8];
  
  if (!digitalRead(CAN0_INT)) {                      // If CAN0_INT pin is low, read receive buffer
    CAN0.readMsgBuf(&rxId, &len, rxBuf);      // Read data: len = data length, buf = data byte(s)

    sprintf(msgString, "Standard ID: 0x%.3lX, DLC: %1d, Data: ", rxId, len);
    Serial.print(msgString);

    for (byte i = 0; i < len; i++) {
      sprintf(msgString, " 0x%.2X", rxBuf[i]);
      Serial.print(msgString);
    }
    Serial.println("");
    switch(rxBuf[1]){
      case 0x41:
      {
        interpretPIDAndSend(rxBuf);
        break;
      }
      case 0x43:
      {
        interpretDTCAndSend(&(rxBuf[0]));
        interpretDTCAndSend(&(rxBuf[2]));
        break;
      }
    }
  }
}
void interpretDTCAndSend(unsigned char* rxBuf){
  uint8_t secondCode = (rxBuf[3]>>4) & 0x3;
  uint8_t thirdCode = (rxBuf[3]) & 0xF;
  uint8_t fourthCode = (rxBuf[4]>>4) & 0xF;
  uint8_t fifthCode =  (rxBuf[4]) & 0xF;
  switch ((rxBuf[3]>>6)& 0x3){
    
    case 0x00:
      {
        DTCFirstCode = 'P';
        PrintOBD2DTC(OBD2_HEADER,OBD2_CATEGORY_DTC,DTCFirstCode,secondCode,thirdCode,fourthCode,fifthCode,0);
        break;
      }
    case 0x01:
      {
        DTCFirstCode = 'C';
        PrintOBD2DTC(OBD2_HEADER,OBD2_CATEGORY_DTC,DTCFirstCode,secondCode,thirdCode,fourthCode,fifthCode,0);
        break;
      }
    case 0x02:
      {
        DTCFirstCode = 'B';
        PrintOBD2DTC(OBD2_HEADER,OBD2_CATEGORY_DTC,DTCFirstCode,secondCode,thirdCode,fourthCode,fifthCode,0);
        break;
      }
    case 0x03:
      {
        DTCFirstCode = 'U';
        PrintOBD2DTC(OBD2_HEADER,OBD2_CATEGORY_DTC,DTCFirstCode,secondCode,thirdCode,fourthCode,fifthCode,0);
        break;
      }
    default:
      {
        Serial.println("OBD2____,DTC,DEFAULT,0");
        break;
      }
  }
}

void requestPID(unsigned char __pid)
{
  unsigned char tmp[8] = {0x02, 0x01, __pid, 0, 0, 0, 0, 0};

  byte sndStat = CAN0.sendMsgBuf(CAN_ID_PID, 0, 8, tmp);

  if (sndStat == CAN_OK) {
    Serial.print("PID sent: 0x");
    Serial.println(__pid, HEX);
  }
  else {
    Serial.println("Error Sending Message...");
    
      delay(1);
  }
}
void interpretPIDAndSend(unsigned char* rxBuf){
  switch (rxBuf[2]) {
      case PID_ENGINE_REF_TORQUE:
        {
          uint32_t temp =  256 * rxBuf[3] + rxBuf[4];
          Serial.print("Engine Reference Torque (Nm) : ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_ENGINE_TORQUE_PERCENTAGE:
        {
          float temp =  rxBuf[3] - 125;
          Serial.print("Actual Engine Percent Torque (%) : ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_ENGINE_FUEL_RATE:
        {
          uint32_t temp = (256 * rxBuf[3] + rxBuf[4])/20;
          Serial.print("Engine Fuel Rate (L/h) : ");
          Serial.println(temp, DEC);
          PrintOBD2Data(OBD2_HEADER,OBD2_CATEGORY_STATUS,PID_ENGINE_FUEL_RATE,temp);
          break;
        }

      case PID_ENGINE_OIL_TEMP:
        {
          float temp = rxBuf[3] - 40;
          Serial.print("Engine Oil Temperature (degC) : ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_HYBRID_BATTERY_PERCENTAGE:
        {
          float temp = (100 * rxBuf[3]) / 255;
          Serial.print("Hybrid Battery Percentage (%) : ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_TIME_SINCE_CODES_CLEARED:
        {
          uint32_t temp = 256 * rxBuf[3] + rxBuf[4];
          Serial.print("Time Since Trouble Code Cleared (min) : ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_TIME_WITH_MIL:
        {
          uint32_t temp = 256 * rxBuf[3] + rxBuf[4];
          Serial.print("Time With MIL (min) : ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_AMBIENT_TEMP:
        {
          float temp = rxBuf[3] - 40 ;
          Serial.print("Ambient Temperature (degC) : ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_ABSOLUTE_ENGINE_LOAD:
        {
          uint32_t temp =100 * (256 * rxBuf[3] + rxBuf[4])/255;
          Serial.print("Absolute Engine Load (%) : ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_CONTROL_MODULE_VOLTAGE:
        {
          float temp =  (256 * rxBuf[3] + rxBuf[4])/1000;
          Serial.print("Control Module Voltage (V) : ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_BAROMETRIC:
        {
          uint16_t temp =  rxBuf[3];
          Serial.print("Barometric Pressure (kPa) : ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_DISTANCE:
        {
          uint32_t temp = (256 * rxBuf[3]) + rxBuf[4];
          Serial.print("Distance Traveled (km) : ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_WARMS_UPS:
        {
          uint16_t temp = rxBuf[3];
          Serial.print("Engine Temperature : ");
          Serial.println(temp, DEC);
          break;
        }
      
      case PID_FUEL_LEVEL:
        {
          float temp = (100 * rxBuf[3])/255;
          Serial.print("OBD2____,STATUS,FUEL_LEVEL,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_EGR_ERROR:
        {
          float temp = (100 * rxBuf[3])/128-100;
          Serial.print("EGR Error (%): ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_COMMANDED_EGR:
        {
          float temp = (100 * rxBuf[3])/255;
          Serial.print("Commanded EGR (%): ");
          Serial.println(temp, DEC);
          break;
        }
      case PID_DISTANCE_WITH_MIL:
        {
          uint32_t temp = 256 * rxBuf[3] + rxBuf[4];
          Serial.print("Distance traveled with MIL on (km): ");
          Serial.println(temp, DEC);
          break;
        }
      
      case PID_RUNTIME:
        {
          uint32_t temp = 256 * rxBuf[3] + rxBuf[4];
          Serial.print("Drive RunTime (s): ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_AUX_INPUT:
        {
          uint8_t temp = rxBuf[3];
          if(rxBuf[3] == 1){
            Serial.println("AUX is in use.");
          }
          else{
            Serial.println("AUX is not in use.");
          } 
          break;
        }

      case PID_MAF_FLOW:
        {
          int16_t temp = (256 * rxBuf[3] + rxBuf[4]) / 4; // 음수값이 있어서 int8_t사용
          Serial.print("MAP Flow (degC): ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_TIMING_ADVANCE:
        {
          int8_t temp = rxBuf[3]/2 - 64; // 음수값이 있어서 int8_t사용
          Serial.print("Timing Advance (TDC): ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_VEHICLE_SPEED:
        {
          uint8_t Vehicle_Speed = rxBuf[3];
          Serial.print("OBD2____,STATUS,VEHICLE_SPEED,");
          Serial.println(Vehicle_Speed,DEC);
          break;
        }
      case PID_FUEL_PRESSURE:
        {
          uint16_t Fuel_Pressure = 3 * rxBuf[3];
          Serial.print("Fuel Pressure (kPa): ");
          Serial.println(Fuel_Pressure,DEC);
          break;
        }

      case PID_ENGINE_LOAD:
        {
          float Engine_Load = ( 100 * rxBuf[3] ) / 255;
          Serial.print("Engine Load (%): ");
          Serial.println(Engine_Load,DEC);
          break;
        }

      case PID_COOLANT_TEMP:
        {
          uint8_t temp;
          temp = rxBuf[3] - 40;
          Serial.print("OBD2____,STATUS,COOLANT_TEMP,");
          Serial.println(temp,DEC);
          break;
        }

      case PID_ENGINE_RPM:
        {
          uint16_t rpm = ((256 * rxBuf[3]) + rxBuf[4]) / 4;;
          // Serial.print("Engine Speed (rpm): ");
          // Serial.println(rpm, DEC);
          Serial.print("OBD2____,STATUS,RPM,");
          Serial.println(rpm,DEC);
          break;
        }




    }
}
void receivePID(unsigned char __pid)
{
  unsigned char rxBuf[8];
  if (!digitalRead(CAN0_INT)) {                      // If CAN0_INT pin is low, read receive buffer
    CAN0.readMsgBuf(&rxId, &len, rxBuf);      // Read data: len = data length, buf = data byte(s)

    sprintf(msgString, "Standard ID: 0x%.3lX, DLC: %1d, Data: ", rxId, len);
    Serial.print(msgString);

    for (byte i = 0; i < len; i++) {
      sprintf(msgString, " 0x%.2X", rxBuf[i]);
      Serial.print(msgString);
    }
    Serial.println("");
    if(rxBuf[2] != __pid){
      Serial.print("PID is not equal rxBuf[2]");
      Serial.print(__pid,HEX);
      Serial.println(rxBuf[2],HEX);
      
    }
    
    switch (rxBuf[2]) {
      case PID_ENGINE_REF_TORQUE:
        {
          uint32_t temp =  256 * rxBuf[3] + rxBuf[4];
          Serial.print("Engine Reference Torque (Nm) : ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_ENGINE_TORQUE_PERCENTAGE:
        {
          float temp =  rxBuf[3] - 125;
          Serial.print("Actual Engine Percent Torque (%) : ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_ENGINE_FUEL_RATE:
        {
          uint32_t temp = (256 * rxBuf[3] + rxBuf[4])/20;
          Serial.print("Engine Fuel Rate (L/h) : ");
          Serial.println(temp, DEC);
          PrintOBD2Data(OBD2_HEADER,OBD2_CATEGORY_STATUS,PID_ENGINE_FUEL_RATE,temp);
          break;
        }

      case PID_ENGINE_OIL_TEMP:
        {
          float temp = rxBuf[3] - 40;
          Serial.print("Engine Oil Temperature (degC) : ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_HYBRID_BATTERY_PERCENTAGE:
        {
          float temp = (100 * rxBuf[3]) / 255;
          Serial.print("Hybrid Battery Percentage (%) : ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_TIME_SINCE_CODES_CLEARED:
        {
          uint32_t temp = 256 * rxBuf[3] + rxBuf[4];
          Serial.print("Time Since Trouble Code Cleared (min) : ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_TIME_WITH_MIL:
        {
          uint32_t temp = 256 * rxBuf[3] + rxBuf[4];
          Serial.print("Time With MIL (min) : ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_AMBIENT_TEMP:
        {
          float temp = rxBuf[3] - 40 ;
          Serial.print("Ambient Temperature (degC) : ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_ABSOLUTE_ENGINE_LOAD:
        {
          uint32_t temp =100 * (256 * rxBuf[3] + rxBuf[4])/255;
          Serial.print("Absolute Engine Load (%) : ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_CONTROL_MODULE_VOLTAGE:
        {
          float temp =  (256 * rxBuf[3] + rxBuf[4])/1000;
          Serial.print("Control Module Voltage (V) : ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_BAROMETRIC:
        {
          uint16_t temp =  rxBuf[3];
          Serial.print("Barometric Pressure (kPa) : ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_DISTANCE:
        {
          uint32_t temp = (256 * rxBuf[3]) + rxBuf[4];
          Serial.print("Distance Traveled (km) : ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_WARMS_UPS:
        {
          uint16_t temp = rxBuf[3];
          Serial.print("Engine Temperature : ");
          Serial.println(temp, DEC);
          break;
        }
      
      case PID_FUEL_LEVEL:
        {
          float temp = (100 * rxBuf[3])/255;
          Serial.print("OBD2____,STATUS,FUEL_LEVEL,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_EGR_ERROR:
        {
          float temp = (100 * rxBuf[3])/128-100;
          Serial.print("EGR Error (%): ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_COMMANDED_EGR:
        {
          float temp = (100 * rxBuf[3])/255;
          Serial.print("Commanded EGR (%): ");
          Serial.println(temp, DEC);
          break;
        }
      case PID_DISTANCE_WITH_MIL:
        {
          uint32_t temp = 256 * rxBuf[3] + rxBuf[4];
          Serial.print("Distance traveled with MIL on (km): ");
          Serial.println(temp, DEC);
          break;
        }
      
      case PID_RUNTIME:
        {
          uint32_t temp = 256 * rxBuf[3] + rxBuf[4];
          Serial.print("Drive RunTime (s): ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_AUX_INPUT:
        {
          uint8_t temp = rxBuf[3];
          if(rxBuf[3] == 1){
            Serial.println("AUX is in use.");
          }
          else{
            Serial.println("AUX is not in use.");
          } 
          break;
        }

      case PID_MAF_FLOW:
        {
          int16_t temp = (256 * rxBuf[3] + rxBuf[4]) / 4; // 음수값이 있어서 int8_t사용
          Serial.print("MAP Flow (degC): ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_TIMING_ADVANCE:
        {
          int8_t temp = rxBuf[3]/2 - 64; // 음수값이 있어서 int8_t사용
          Serial.print("Timing Advance (TDC): ");
          Serial.println(temp, DEC);
          break;
        }

      case PID_VEHICLE_SPEED:
        {
          uint8_t Vehicle_Speed = rxBuf[3];
          Serial.print("OBD2____,STATUS,VEHICLE_SPEED,");
          Serial.println(Vehicle_Speed,DEC);
          break;
        }
      case PID_FUEL_PRESSURE:
        {
          uint16_t Fuel_Pressure = 3 * rxBuf[3];
          Serial.print("Fuel Pressure (kPa): ");
          Serial.println(Fuel_Pressure,DEC);
          break;
        }

      case PID_ENGINE_LOAD:
        {
          float Engine_Load = ( 100 * rxBuf[3] ) / 255;
          Serial.print("Engine Load (%): ");
          Serial.println(Engine_Load,DEC);
          break;
        }

      case PID_COOLANT_TEMP:
        {
          uint8_t temp;
          temp = rxBuf[3] - 40;
          Serial.print("OBD2____,STATUS,COOLANT_TEMP,");
          Serial.println(temp,DEC);
          break;
        }

      case PID_ENGINE_RPM:
        {
          uint16_t rpm = ((256 * rxBuf[3]) + rxBuf[4]) / 4;;
          // Serial.print("Engine Speed (rpm): ");
          // Serial.println(rpm, DEC);
          Serial.print("OBD2____,STATUS,RPM,");
          Serial.println(rpm,DEC);
          break;
        }
    }
  }
}
void requestAndReceivePID(unsigned char pid){
  requestPID(pid);
  delay(OBD_DELAY); //to allow time for ECU to reply
  receiveOBD();
  delay(OBD_DELAY); //to allow time for ECU to reply

}
void requestAndReceiveDTC(){
  requestStoredDTC();
  delay(OBD_DELAY);
  receiveOBD();
  delay(OBD_DELAY);
}
