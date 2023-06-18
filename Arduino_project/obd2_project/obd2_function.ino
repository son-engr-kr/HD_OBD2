
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
    PID_COOLANT_TEMP                    ,
    PID_FUEL_PRESSURE                   ,
    PID_INTAKE_MAP                      ,
    // PID_ENGINE_RPM                      ,
    // PID_VEHICLE_SPEED                   ,
    PID_TIMING_ADVANCE                  ,
    PID_INTAKE_TEMP                     ,
    PID_MAF_FLOW                        ,
    PID_THROTTLE                        ,
    PID_RUNTIME                         ,
    PID_DISTANCE_WITH_MIL               ,
    PID_COMMANDED_EGR                   ,
    PID_EGR_ERROR                       ,
    PID_COMMANDED_EVAPORATIVE_PURGE     ,
    PID_FUEL_LEVEL                      ,
    PID_WARMS_UPS                       ,
    PID_DISTANCE                        ,
    PID_EVAP_SYS_VAPOR_PRESSURE         ,
    PID_BAROMETRIC                      ,
    PID_CATALYST_TEMP_B1S1              ,
    PID_CATALYST_TEMP_B2S1              ,
    PID_CATALYST_TEMP_B1S2              ,
    PID_CATALYST_TEMP_B2S2              ,
    PID_CONTROL_MODULE_VOLTAGE          ,
    PID_ABSOLUTE_ENGINE_LOAD            ,
    PID_AIR_FUEL_EQUIV_RATIO            ,
    PID_RELATIVE_THROTTLE_POS           ,
    PID_AMBIENT_TEMP                    ,
    
    PID_TIME_WITH_MIL                   ,
    PID_TIME_SINCE_CODES_CLEARED        ,
    PID_ETHANOL_FUEL                    ,
    PID_FUEL_RAIL_PRESSURE              ,
    PID_HYBRID_BATTERY_PERCENTAGE       ,
    PID_ENGINE_OIL_TEMP                 ,
    PID_FUEL_INJECTION_TIMING           ,
    PID_ENGINE_TORQUE_DEMANDED          ,
    PID_ENGINE_TORQUE_PERCENTAGE        ,
    PID_ENGINE_REF_TORQUE               ,
  };
unsigned char pidMidSignificant[] = {
    PID_SHORT_TERM_FUEL_TRIM_1          ,
    PID_LONG_TERM_FUEL_TRIM_1           ,
    PID_SHORT_TERM_FUEL_TRIM_2          ,
    PID_LONG_TERM_FUEL_TRIM_2           ,
    PID_ENGINE_LOAD                     ,
    PID_ODOMETER                        ,
    PID_ENGINE_FUEL_RATE                ,
    PID_AUX_INPUT                       ,

    PID_ABSOLUTE_THROTTLE_POS_B         ,
    PID_ABSOLUTE_THROTTLE_POS_C         ,
    PID_ACC_PEDAL_POS_D                 ,
    PID_ACC_PEDAL_POS_E                 ,
    PID_ACC_PEDAL_POS_F                 ,
    PID_COMMANDED_THROTTLE_ACTUATOR     ,
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
      case PID_SHORT_TERM_FUEL_TRIM_1:
        {
          int32_t temp =  (100 * rxBuf[3]) / 128 - 100; ///////
          Serial.print("OBD2____,STATUS,SHORT_TERM_FUEL_TRIM_1,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_LONG_TERM_FUEL_TRIM_1:
        {
          int32_t temp =  (100 * rxBuf[3]) / 128 - 100;////
          Serial.print("OBD2____,STATUS,LONG_TERM_FUEL_TRIM_1,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_SHORT_TERM_FUEL_TRIM_2:
        {
          int32_t temp =  (100 * rxBuf[3]) / 128 - 100;////
          Serial.print("OBD2____,STATUS,SHORT_TERM_FUEL_TRIM_2,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_LONG_TERM_FUEL_TRIM_2:
        {
          int32_t temp =  (100 * rxBuf[3]) / 128 - 100;////
          Serial.print("OBD2____,STATUS,LONG_TERM_FUEL_TRIM_2,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_INTAKE_MAP:
        {
          uint32_t temp =  rxBuf[3];
          Serial.print("OBD2____,STATUS,INTAKE_MAP,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_INTAKE_TEMP:
        {
          int32_t temp =  rxBuf[3] - 40;////
          Serial.print("OBD2____,STATUS,INTAKE_TEMP,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_THROTTLE:
        {
          uint32_t temp =  (100 * rxBuf[3]) / 255 ;////
          Serial.print("OBD2____,STATUS,THROTTLE,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_COMMANDED_EVAPORATIVE_PURGE:
        {
          uint32_t temp =  (100 * rxBuf[3]) / 255;////
          Serial.print("OBD2____,STATUS,COMMANDED_EVAPORATIVE_PURGE,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_EVAP_SYS_VAPOR_PRESSURE:
        {
          uint32_t temp =  (256 * rxBuf[3] + rxBuf[4]) / 4 ;////
          Serial.print("OBD2____,STATUS,EVAP_SYS_VAPOR_PRESSURE,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_CATALYST_TEMP_B1S1:
        {
          int32_t temp =  (256 * rxBuf[3] + rxBuf[4]) / 10 - 40 ;////
          Serial.print("OBD2____,STATUS,CATALYST_TEMP_B1S1,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_CATALYST_TEMP_B2S1:
        {
          int32_t temp =  (256 * rxBuf[3] + rxBuf[4]) / 10 - 40 ;////
          Serial.print("OBD2____,STATUS,CATALYST_TEMP_B2S1,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_CATALYST_TEMP_B1S2:
        {
          int32_t temp =  (256 * rxBuf[3] + rxBuf[4]) / 10 - 40 ;////
          Serial.print("OBD2____,STATUS,CATALYST_TEMP_B1S2,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_CATALYST_TEMP_B2S2:
        {
          int32_t temp = (256 * rxBuf[3] + rxBuf[4]) / 10 - 40 ;////
          Serial.print("OBD2____,STATUS,CATALYST_TEMP_B2S2,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_AIR_FUEL_EQUIV_RATIO:
        {
          uint32_t temp = 2 * (256 * rxBuf[3] + rxBuf[4]) / 65536 ;////
          Serial.print("OBD2____,STATUS,AIR_FUEL_EQUIV_RATIO,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_RELATIVE_THROTTLE_POS:
        {
          uint32_t temp = (100 * rxBuf[3]) / 255 ;////
          Serial.print("OBD2____,STATUS,RELATIVE_THROTTLE_POS,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_ABSOLUTE_THROTTLE_POS_B:
        {
          uint32_t temp = (100 * rxBuf[3]) / 255 ;////
          Serial.print("OBD2____,STATUS,ABSOLUTE_THROTTLE_POS_B,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_ABSOLUTE_THROTTLE_POS_C:
        {
          uint32_t temp = (100 * rxBuf[3]) / 255 ;////
          Serial.print("OBD2____,STATUS,ABSOLUTE_THROTTLE_POS_C,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_ACC_PEDAL_POS_D:
        {
          uint32_t temp = (100 * rxBuf[3]) / 255 ;////
          Serial.print("OBD2____,STATUS,ACC_PEDAL_POS_D,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_ACC_PEDAL_POS_E:
        {
          uint32_t temp = (100 * rxBuf[3]) / 255 ;////
          Serial.print("OBD2____,STATUS,ACC_PEDAL_POS_E,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_ACC_PEDAL_POS_F:
        {
          uint32_t temp = (100 * rxBuf[3]) / 255 ;////
          Serial.print("OBD2____,STATUS,ACC_PEDAL_POS_F,");
          Serial.println(temp, DEC);
          break;
        }  

      case PID_COMMANDED_THROTTLE_ACTUATOR:
        {
          uint32_t temp = (100 * rxBuf[3]) / 255 ;////
          Serial.print("OBD2____,STATUS,COMMANDED_THROTTLE_ACTUATOR,");
          Serial.println(temp, DEC);
          break;
        }  

      case PID_ETHANOL_FUEL:
        {
          uint32_t temp = (100 * rxBuf[3]) / 255 ;////
          Serial.print("OBD2____,STATUS,PID_ETHANOL_FUEL,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_FUEL_RAIL_PRESSURE:
        {
          uint32_t temp = 10 * (256 * rxBuf[3] + rxBuf[4]);////
          Serial.print("OBD2____,STATUS,FUEL_RAIL_PRESSURE,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_FUEL_INJECTION_TIMING:
        {
          int32_t temp =  256 * rxBuf[3] + rxBuf[4];////
          Serial.print("OBD2____,STATUS,FUEL_INJECTION_TIMING,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_ENGINE_TORQUE_DEMANDED:
        {
          int32_t temp = (256 * rxBuf[3] + rxBuf[4]) / 128 - 210;////
          Serial.print("OBD2____,STATUS,ENGINE_TORQUE_DEMANDED,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_ENGINE_REF_TORQUE:
        {
          uint32_t temp =  256 * rxBuf[3] + rxBuf[4];
          // Serial.print("Engine Reference Torque (Nm) : ");
          Serial.print("OBD2____,STATUS,ENGINE_REF_TORQUE,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_ENGINE_TORQUE_PERCENTAGE:
        {
          float temp =  rxBuf[3] - 125;
          // Serial.print("Actual Engine Percent Torque (%) : ");
          Serial.print("OBD2____,STATUS,ACTUAL_ENGINE_TORQUE_PERCENTAGE,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_ENGINE_FUEL_RATE:
        {
          uint32_t temp = (256 * rxBuf[3] + rxBuf[4])/20;
          // Serial.print("Engine Fuel Rate (L/h) : ");
          Serial.print("OBD2____,STATUS,ENGINE_FUEL_RATE,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_ENGINE_OIL_TEMP:
        {
          float temp = rxBuf[3] - 40;
          // Serial.print("Engine Oil Temperature (degC) : ");
          Serial.print("OBD2____,STATUS,ENGINE_OIL_TEMP,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_HYBRID_BATTERY_PERCENTAGE:
        {
          float temp = (100 * rxBuf[3]) / 255;
          // Serial.print("Hybrid Battery Percentage (%) : ");
          Serial.print("OBD2____,STATUS,HYBRID_BATTERY_PERCENTAGE,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_TIME_SINCE_CODES_CLEARED:
        {
          uint32_t temp = 256 * rxBuf[3] + rxBuf[4];
          // Serial.print("Time Since Trouble Code Cleared (min) : ");
          Serial.print("OBD2____,STATUS,TIME_SINCE_CODES_CLEARED,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_TIME_WITH_MIL:
        {
          uint32_t temp = 256 * rxBuf[3] + rxBuf[4];
          // Serial.print("Time With MIL (min) : ");
          Serial.print("OBD2____,STATUS,TIME_SINCE_CODES_CLEARED,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_AMBIENT_TEMP:
        {
          float temp = rxBuf[3] - 40 ;
          // Serial.print("Ambient Temperature (degC) : ");
          Serial.print("OBD2____,STATUS,AMBIENT_TEMP,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_ABSOLUTE_ENGINE_LOAD:
        {
          uint32_t temp =100 * (256 * rxBuf[3] + rxBuf[4])/255;
          // Serial.print("Absolute Engine Load (%) : ");
          Serial.print("OBD2____,STATUS,ABSOLUTE_ENGINE_LOAD,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_CONTROL_MODULE_VOLTAGE:
        {
          float temp =  (256 * rxBuf[3] + rxBuf[4])/1000;
          // Serial.print("Control Module Voltage (V) : ");
          Serial.print("OBD2____,STATUS,CONTROL_MODULE_VOLTAGE,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_BAROMETRIC:
        {
          uint16_t temp =  rxBuf[3];
          // Serial.print("Barometric Pressure (kPa) : ");
          Serial.print("OBD2____,STATUS,BAROMETRIC_PRESSURE,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_DISTANCE:
        {
          uint32_t temp = (256 * rxBuf[3]) + rxBuf[4];
          // Serial.print("Distance Traveled (km) : ");
          Serial.print("OBD2____,STATUS,DISTANCE,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_WARMS_UPS:
        {
          uint16_t temp = rxBuf[3];
          // Serial.print("Engine Temperature : ");
          Serial.print("OBD2____,STATUS,ENGINE_TEMP,");
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
          // Serial.print("EGR Error (%): ");
          Serial.print("OBD2____,STATUS,EGR_ERROR,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_COMMANDED_EGR:
        {
          float temp = (100 * rxBuf[3])/255;
          // Serial.print("Commanded EGR (%): ");
          Serial.print("OBD2____,STATUS,COMMANDED_EGR,");
          Serial.println(temp, DEC);
          break;
        }
      case PID_DISTANCE_WITH_MIL:
        {
          uint32_t temp = 256 * rxBuf[3] + rxBuf[4];
          // Serial.print("Distance traveled with MIL on (km): ");
          Serial.print("OBD2____,STATUS,DISTANCE_TRAVEL_WITH_MIL,");
          Serial.println(temp, DEC);
          break;
        }
      
      case PID_RUNTIME:
        {
          uint32_t temp = 256 * rxBuf[3] + rxBuf[4];
          // Serial.print("Drive RunTime (s): ");
          Serial.print("OBD2____,STATUS,DRIVE_RUNTIME,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_AUX_INPUT:
        {
          uint8_t temp = rxBuf[3];
          if(rxBuf[3] == 1){
            // Serial.println("AUX is in use.");
            Serial.println("OBD2____,STATUS,Aux is in use,1");
          }
          else{
            // Serial.println("AUX is not in use.");
            Serial.println("OBD2____,STATUS,Aux is not in use,0");
          } 
          break;
        }

      case PID_MAF_FLOW:
        {
          int16_t temp = (256 * rxBuf[3] + rxBuf[4]) / 4; // 음수값이 있어서 int8_t사용
          // Serial.print("MAP Flow (degC): ");
          Serial.print("OBD2____,STATUS,MAP_FLOW,");
          Serial.println(temp, DEC);
          break;
        }

      case PID_TIMING_ADVANCE:
        {
          int8_t temp = rxBuf[3]/2 - 64; // 음수값이 있어서 int8_t사용
          // Serial.print("Timing Advance (TDC): ");
          Serial.print("OBD2____,STATUS,TIMING_ADVANCE,");
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
          // Serial.print("Fuel Pressure (kPa): ");
          Serial.print("OBD2____,STATUS,FUEL_PRESSURE,");
          Serial.println(Fuel_Pressure,DEC);
          break;
        }

      case PID_ENGINE_LOAD:
        {
          float Engine_Load = ( 100 * rxBuf[3] ) / 255;
          // Serial.print("Engine Load (%): ");
          Serial.print("OBD2____,STATUS,ENGINE_LOAD,");
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
          uint16_t rpm = ((256 * rxBuf[3]) + rxBuf[4]) / 4;
          // Serial.print("Engine Speed (rpm): ");
          Serial.print("OBD2____,STATUS,RPM,");
          Serial.println(rpm,DEC);
          break;
        }
      case PID_ODOMETER:
        {
          uint32_t temp = ((256*256*256 * rxBuf[3])+(256*256 * rxBuf[4])+(256 * rxBuf[5]) + rxBuf[6]) / 10;
          // Serial.print("Engine Speed (rpm): ");
          Serial.print("OBD2____,STATUS,ODOMETER,");
          Serial.println(temp,DEC);
          break;
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
