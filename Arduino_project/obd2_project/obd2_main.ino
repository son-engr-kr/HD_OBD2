
void setup()
{
  Serial.begin(115200);

  // Initialize MCP2515 running at 16MHz with a baudrate of 500kb/s and the masks and filters disabled.
  if (CAN0.begin(MCP_STDEXT, CAN_500KBPS, MCP_8MHZ) == CAN_OK) { //< -------- - CHANGE if using different board
    Serial.println("MCP2515 Initialized Successfully!");
  }
  else {
    Serial.println("Error Initializing MCP2515...");
    while (1){
      delay(100);
    };
  }

  //initialise mask and filter to allow only receipt of 0x7xx CAN IDs
  CAN0.init_Mask(0, 0, 0x07000000);              // Init first mask...
  CAN0.init_Mask(1, 0, 0x07000000);              // Init second mask...

  
  for (uint8_t i = 0; i < 6; ++i) {
    CAN0.init_Filt(i, 0, 0x07000000);           //Init filters
  }
  
  CAN0.setMode(MCP_NORMAL);                     // Set operation mode to normal so the MCP2515 sends acks to received data.

  pinMode(CAN0_INT, INPUT);                    // Configuring pin for /INT input

  Serial.println("Sending and Receiving OBD-II_PIDs Example...");



  // Serial.println("OBD2____,STATUS,Test,0");
  // Serial.println("OBD2____,STATUS,RPM,1234");
  // Serial.println("OBD2____,STATUS,COOLANT_TEMP,56");
  // Serial.println("OBD2____,STATUS,VEHICLE_SPEED,78");
}

void loop()
{
  requestAndReceivePID(PID_ENGINE_RPM);
  requestAndReceivePID(PID_VEHICLE_SPEED);
  if(requestCount % 100 == 0){
    for(int idx = 0; idx < sizeof(pidLowSignificant)/sizeof(unsigned char); idx++){
      requestAndReceivePID(pidLowSignificant[idx]);
      requestAndReceivePID(PID_ENGINE_RPM);
      requestAndReceivePID(PID_VEHICLE_SPEED);
    }
  }
  if(requestCount % 1000 == 0){
    requestAndReceiveDTC();
  }

  // ClearDTCandMIL();


  // delay(500);

  requestCount++;
}