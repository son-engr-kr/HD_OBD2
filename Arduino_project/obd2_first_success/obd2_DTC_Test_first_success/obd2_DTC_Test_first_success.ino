#define OBD2_HEADER "OBD2____"
#define OBD2_CATEGORY_STATUS "STATUS"
#define OBD2_CATEGORY_DTC "DTC"
int rxBuf[2] = {69,73};
// rxBuf[0] = 5;
// rxBuf[1] = 73;
char DTCFirstCode = 'A';

char buffer[1000];
uint8_t secondCode = (rxBuf[0]>>4) & 0x3;
uint8_t thirdCode = (rxBuf[0]) & 0xF;
uint8_t fourthCode = (rxBuf[1]>>4) & 0xF;
uint8_t fifthCode =  (rxBuf[1]) & 0xF;


void PrintOBD2Data(char* HEADER, char* CATEGORY, char* pidname, double value){
  sprintf(buffer,"%s,%s,%s,%d",HEADER,CATEGORY,pidname,value);
    Serial.println(buffer);
}
void PrintOBD2DTC(char* HEADER, char* CATEGORY,char char1, int num2, int num3, int num4, int num5 , double value){
  sprintf(buffer,"%s,%s,%c%d%d%d%d,%d",HEADER,CATEGORY,
  char1,num2,num3,num4,num5,value );
  Serial.println(buffer);
}


void setup() {
  // put your setup code here, to run once:
  Serial.begin(115200);
  // Serial.print("afaf");
  PrintOBD2Data(OBD2_HEADER,OBD2_CATEGORY_DTC,"Clear_Test_Sending_fail",0);
  if(((rxBuf[0]>>6)& 0x03) == 0x01){DTCFirstCode = 'C';}
  PrintOBD2DTC(OBD2_HEADER,OBD2_CATEGORY_DTC,DTCFirstCode,secondCode,thirdCode,fourthCode,fifthCode,0);
  // PrintOBD2Data(OBD2_HEADER,OBD2_CATEGORY_DTC,"Clear_Test_Sending_fail",0);
}

void loop() {
  // put your main code here, to run repeatedly:
  // PrintOBD2Data(OBD2_HEADER,OBD2_CATEGORY_DTC,"Clear_Test_Sending_fail",0);
  // if(rxBuf[0]>>6& 0x3){DTCFirstCode = 'P';}
  // PrintOBD2DTC(OBD2_HEADER,OBD2_CATEGORY_DTC,DTCFirstCode,secondCode,thirdCode,fourthCode,fifthCode,0);
  PrintOBD2Data(OBD2_HEADER,OBD2_CATEGORY_DTC,"Clear_Test_Sending_fail",0);
  delay(100);
  if(((rxBuf[0]>>6)& 0x03) == 0x01){DTCFirstCode = 'C';}
  PrintOBD2DTC(OBD2_HEADER,OBD2_CATEGORY_DTC,DTCFirstCode,secondCode,thirdCode,fourthCode,fifthCode,0);
  delay(100);
}
