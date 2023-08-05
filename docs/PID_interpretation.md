 ## 활용할 만한 자료들
- AUX 상태 확인가능
- Distance with MIL : MIL이 켜진 상태로의 주행 거리(MIL : Malfunction Indicator Lamp) 
- Fuel Level: 남은 연료의 양을 %로 보여줌-연료소비율 및 주행거리 등과 관련된 정보를 계산하는데 도움 될 수도 있음.
- Ambient Temp : 주변온도
- Engine Fuel Rate : 현재 엔진 연료 소비율
- Control moudle voltage : 남은 배터리
 
 ## 안쓰는거
- #define PID_SHORT_TERM_FUEL_TRIM_1 0x06
- #define PID_LONG_TERM_FUEL_TRIM_1 0x07
- #define PID_SHORT_TERM_FUEL_TRIM_2 0x08
- #define PID_LONG_TERM_FUEL_TRIM_2 0x09
- #define PID_INTAKE_MAP 0x0B
- #define PID_INTAKE_TEMP 0x0F
- #define PID_THROTTLE 0x11
- #define PID_COMMANDED_EVAPORATIVE_PURGE 0x2E
- #define PID_EVAP_SYS_VAPOR_PRESSURE 0x32
- #define PID_CATALYST_TEMP_B1S1 0x3C
- #define PID_CATALYST_TEMP_B2S1 0x3D
- #define PID_CATALYST_TEMP_B1S2 0x3E
- #define PID_CATALYST_TEMP_B2S2 0x3F
- #define PID_AIR_FUEL_EQUIV_RATIO 0x44
- #define PID_RELATIVE_THROTTLE_POS 0x45
- #define PID_ABSOLUTE_THROTTLE_POS_B 0x47
- #define PID_ABSOLUTE_THROTTLE_POS_C 0x48
- #define PID_ACC_PEDAL_POS_D 0x49
- #define PID_ACC_PEDAL_POS_E 0x4A
- #define PID_ACC_PEDAL_POS_F 0x4B
- #define PID_COMMANDED_THROTTLE_ACTUATOR 0x4C
- #define PID_ETHANOL_FUEL 0x52
- #define PID_FUEL_RAIL_PRESSURE 0x59
- #define PID_FUEL_INJECTION_TIMING 0x5D
- #define PID_ENGINE_TORQUE_DEMANDED 0x61
 
  PID(0x06) SHORT_TERM_FUEL_TRIM_1 :  주 연료 트림(short-term fuel trim) 값 중 하나를 나타냅니다. 주 연료 트림은 엔진의 연소 효율과 배출 시스템의 상태를 모니터링하기 위해 사용되는 매개 변수입니다.SHORT_TERM_FUEL_TRIM_1은 일반적으로 1번째 뱅크 또는 A/F 센서(Bank 1, Sensor 1)와 관련된 연료 트림 값을 나타냅니다

  PID(0x0B) Intake Manifold Absolute Pressure : 흡기 매니폴드 절대 압력을 나타냅니다. 이 매개 변수는 엔진의 흡기 매니폴드에서의 압력을 측정, 흡기 매니폴드는 엔진의 실린더로 공기와 연료 혼합물을 공급하는 부분입니다.