// Service 01 PIDs (more detail: https://en.wikipedia.org/wiki/OBD-II_PIDs)
#define PID_ENGINE_LOAD 0x04
#define PID_COOLANT_TEMP 0x05
#define PID_SHORT_TERM_FUEL_TRIM_1 0x06
#define PID_LONG_TERM_FUEL_TRIM_1 0x07
#define PID_SHORT_TERM_FUEL_TRIM_2 0x08
#define PID_LONG_TERM_FUEL_TRIM_2 0x09
#define PID_FUEL_PRESSURE 0x0A
#define PID_INTAKE_MAP 0x0B
#define PID_ENGINE_RPM  0x0C
#define PID_VEHICLE_SPEED 0x0D
#define PID_TIMING_ADVANCE 0x0E
#define PID_INTAKE_TEMP 0x0F
#define PID_MAF_FLOW 0x10
#define PID_THROTTLE 0x11
#define PID_AUX_INPUT 0x1E
#define PID_RUNTIME 0x1F
#define PID_DISTANCE_WITH_MIL 0x21
#define PID_COMMANDED_EGR 0x2C
#define PID_EGR_ERROR 0x2D
#define PID_COMMANDED_EVAPORATIVE_PURGE 0x2E
#define PID_FUEL_LEVEL 0x2F
#define PID_WARMS_UPS 0x30
#define PID_DISTANCE 0x31
#define PID_EVAP_SYS_VAPOR_PRESSURE 0x32
#define PID_BAROMETRIC 0x33
#define PID_CATALYST_TEMP_B1S1 0x3C
#define PID_CATALYST_TEMP_B2S1 0x3D
#define PID_CATALYST_TEMP_B1S2 0x3E
#define PID_CATALYST_TEMP_B2S2 0x3F
#define PID_CONTROL_MODULE_VOLTAGE 0x42
#define PID_ABSOLUTE_ENGINE_LOAD 0x43
#define PID_AIR_FUEL_EQUIV_RATIO 0x44
#define PID_RELATIVE_THROTTLE_POS 0x45
#define PID_AMBIENT_TEMP 0x46
#define PID_ABSOLUTE_THROTTLE_POS_B 0x47
#define PID_ABSOLUTE_THROTTLE_POS_C 0x48
#define PID_ACC_PEDAL_POS_D 0x49
#define PID_ACC_PEDAL_POS_E 0x4A
#define PID_ACC_PEDAL_POS_F 0x4B
#define PID_COMMANDED_THROTTLE_ACTUATOR 0x4C
#define PID_TIME_WITH_MIL 0x4D
#define PID_TIME_SINCE_CODES_CLEARED 0x4E
#define PID_ETHANOL_FUEL 0x52
#define PID_FUEL_RAIL_PRESSURE 0x59
#define PID_HYBRID_BATTERY_PERCENTAGE 0x5B
#define PID_ENGINE_OIL_TEMP 0x5C
#define PID_FUEL_INJECTION_TIMING 0x5D
#define PID_ENGINE_FUEL_RATE 0x5E
#define PID_ENGINE_TORQUE_DEMANDED 0x61
#define PID_ENGINE_TORQUE_PERCENTAGE 0x62
#define PID_ENGINE_REF_TORQUE 0x63
//----------------------------------------------

#define CAN_ID_PID 0x7DF //OBD-II CAN frame ID

#include <mcp_can.h>
#include <SPI.h>

#define CAN0_INT 2                              // Set INT to pin 2  <--------- CHANGE if using different pin number
MCP_CAN CAN0(9);                               // Set CS to pin 10 <--------- CHANGE if using different pin number

long unsigned int rxId;
unsigned char len = 0;
unsigned char rxBuf[8];
char msgString[128];                        // Array to store serial string

void sendPID(unsigned char __pid)
{
  unsigned char tmp[8] = {0x02, 0x01, __pid, 0, 0, 0, 0, 0};

  byte sndStat = CAN0.sendMsgBuf(CAN_ID_PID, 0, 8, tmp);

  if (sndStat == CAN_OK) {
    Serial.print("PID sent: 0x");
    Serial.println(__pid, HEX);
  }
  else {
    Serial.println("Error Sending Message...");
  }
}

void receivePID(unsigned char __pid)
{
    if (!digitalRead(CAN0_INT)) {                      // If CAN0_INT pin is low, read receive buffer
    CAN0.readMsgBuf(&rxId, &len, rxBuf);      // Read data: len = data length, buf = data byte(s)

    sprintf(msgString, "Standard ID: 0x%.3lX, DLC: %1d, Data: ", rxId, len);
    Serial.print(msgString);

    for (byte i = 0; i < len; i++) {
      sprintf(msgString, " 0x%.2X", rxBuf[i]);
      Serial.print(msgString);
    }
    Serial.println("");


    switch (__pid) {
      case PID_COOLANT_TEMP:
        if(rxBuf[2] == PID_COOLANT_TEMP){
          uint8_t temp;
          temp = rxBuf[3] - 40;
          Serial.print("Engine Coolant Temp (degC): ");
          Serial.println(temp, DEC);
        }
      break;

      case PID_ENGINE_RPM:
        if(rxBuf[2] == PID_ENGINE_RPM){
          uint16_t rpm;
          rpm = ((256 * rxBuf[3]) + rxBuf[4]) / 4;
          Serial.print("Engine Speed (rpm): ");
          Serial.println(rpm, DEC);
        }
      break;
    }
  }
}

void setup()
{
  Serial.begin(115200);

  // Initialize MCP2515 running at 16MHz with a baudrate of 500kb/s and the masks and filters disabled.
  if (CAN0.begin(MCP_STDEXT, CAN_500KBPS, MCP_8MHZ) == CAN_OK) { //< -------- - CHANGE if using different board
    Serial.println("MCP2515 Initialized Successfully!");
  }
  else {
    Serial.println("Error Initializing MCP2515...");
    while (1);
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
}

void loop()
{
  //request coolant temp
  sendPID(PID_COOLANT_TEMP);

  delay(40); //to allow time for ECU to reply

  receivePID(PID_COOLANT_TEMP);

  //request engine speed
  sendPID (PID_ENGINE_RPM);

  delay(40); //to allow time for ECU to reply

  receivePID(PID_ENGINE_RPM);

  //abitrary loop delay
  delay(40);

  delay(500);

}