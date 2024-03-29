// Service 01 PIDs (more detail: https://en.wikipedia.org/wiki/OBD-II_PIDs)
// 설정 참고 https://ttuk-ttak.tistory.com/31
#define PID_ENGINE_LOAD                     0x04//(%), mid
#define PID_COOLANT_TEMP                    0x05//low
#define PID_SHORT_TERM_FUEL_TRIM_1          0x06//-
#define PID_LONG_TERM_FUEL_TRIM_1           0x07//-
#define PID_SHORT_TERM_FUEL_TRIM_2          0x08//-
#define PID_LONG_TERM_FUEL_TRIM_2           0x09//-
#define PID_FUEL_PRESSURE                   0x0A//mid
#define PID_INTAKE_MAP                      0x0B//-
#define PID_ENGINE_RPM                      0x0C//high
#define PID_VEHICLE_SPEED                   0x0D//high
#define PID_TIMING_ADVANCE                  0x0E//점화시기 low
#define PID_INTAKE_TEMP                     0x0F//-
#define PID_MAF_FLOW                        0x10//mass air flow, high
#define PID_THROTTLE                        0x11//high //-
#define PID_AUX_INPUT                       0x1E//mid
#define PID_RUNTIME                         0x1F//mid
#define PID_DISTANCE_WITH_MIL               0x21//malfunction indicate lamp, low
#define PID_COMMANDED_EGR                   0x2C//디젤
#define PID_EGR_ERROR                       0x2D//디젤
#define PID_COMMANDED_EVAPORATIVE_PURGE     0x2E//-
#define PID_FUEL_LEVEL                      0x2F//-low
#define PID_WARMS_UPS                       0x30//엔진온도, low
#define PID_DISTANCE                        0x31//low, 무슨 거리인지 확인 필요
#define PID_EVAP_SYS_VAPOR_PRESSURE         0x32//-
#define PID_BAROMETRIC                      0x33//kPa, low
#define PID_CATALYST_TEMP_B1S1              0x3C//-
#define PID_CATALYST_TEMP_B2S1              0x3D//-
#define PID_CATALYST_TEMP_B1S2              0x3E//-
#define PID_CATALYST_TEMP_B2S2              0x3F//-
#define PID_CONTROL_MODULE_VOLTAGE          0x42//mid
#define PID_ABSOLUTE_ENGINE_LOAD            0x43//HIGH
#define PID_AIR_FUEL_EQUIV_RATIO            0x44//-
#define PID_RELATIVE_THROTTLE_POS           0x45//-
#define PID_AMBIENT_TEMP                    0x46//low
#define PID_ABSOLUTE_THROTTLE_POS_B         0x47//-
#define PID_ABSOLUTE_THROTTLE_POS_C         0x48//-
#define PID_ACC_PEDAL_POS_D                 0x49//-
#define PID_ACC_PEDAL_POS_E                 0x4A//-
#define PID_ACC_PEDAL_POS_F                 0x4B//-
#define PID_COMMANDED_THROTTLE_ACTUATOR     0x4C//-
#define PID_TIME_WITH_MIL                   0x4D
#define PID_TIME_SINCE_CODES_CLEARED        0x4E
#define PID_ETHANOL_FUEL                    0x52//-
#define PID_FUEL_RAIL_PRESSURE              0x59//-
#define PID_HYBRID_BATTERY_PERCENTAGE       0x5B
#define PID_ENGINE_OIL_TEMP                 0x5C
#define PID_FUEL_INJECTION_TIMING           0x5D//-
#define PID_ENGINE_FUEL_RATE                0x5E
#define PID_ENGINE_TORQUE_DEMANDED          0x61//-
#define PID_ENGINE_TORQUE_PERCENTAGE        0x62
#define PID_ENGINE_REF_TORQUE               0x63
#define PID_ODOMETER                        0xA6//mid

//#define PID_REQUEST_DTC 0x01

#define OBD2_HEADER "OBD2____"
#define OBD2_CATEGORY_STATUS "STATUS"
#define OBD2_CATEGORY_DTC "DTC"
//----------------------------------------------

#define CAN_ID_PID 0x7DF //OBD-II CAN frame ID
#define CAN0_INT 2                              // Set INT to pin 2  <--------- CHANGE if using different pin number

#define OBD_DELAY 10