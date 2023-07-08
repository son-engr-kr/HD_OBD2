#include "WiFiEsp.h"

// Emulate Serial1 on pins 6/7 if not present
#ifndef HAVE_HWSERIAL1
#include "SoftwareSerial.h"
// SoftwareSerial Serial1(2, 3); // RX, TX
#endif

char ssid[] = "OBD2TESTWIFI";  // your network SSID (name)
char pass[] = "obd2obd2";      // your network password
int status = WL_IDLE_STATUS;   // the Wifi radio's status

WiFiEspServer server(400);
WiFiEspClient client;
void setup() {
  // initialize serial for debugging
  Serial.begin(9600);
  // initialize serial for ESP module
  Serial1.begin(115200);
  // initialize ESP module
  WiFi.init(&Serial1);

  // check for the presence of the shield
  if (WiFi.status() == WL_NO_SHIELD) {
    Serial.println("WiFi shield not present");
    // don't continue
    while (true)
      ;
  }

  // attempt to connect to WiFi network
  while (status != WL_CONNECTED) {
    Serial.print("Attempting to connect to WPA SSID: ");
    Serial.println(ssid);
    // Connect to WPA/WPA2 network
    status = WiFi.begin(ssid, pass);
  }

  Serial.println("You're connected to the network");

  printWifiStatus();

  server.begin();
}

int timeout = 0;
char temp[32] = {
  0x00,
};
char header[4] = {};

void loop() {

  timeout += 1;
  while(!client.connected()){
    client = server.available();
    delay(10);
  }
  Serial.println("Client Connected");
  while(client.connected()){
    boolean bDataRead = false;
    while (client.available() > 0) {
      char ch = client.read();
      Serial.write(ch);
      bDataRead = true;
    }
    if (bDataRead == true) {
      Serial.println();
    }

    long bodySize = 1234567;
    // byte bodySizeByte[4] = {};
    header[0] = bodySize & 0xff;
    header[1] = bodySize>>8 & 0xff;
    header[2] = bodySize>>16 & 0xff;
    header[3] = bodySize>>24 & 0xff;

    
    // c.print(header);
    int wqd = client.write(header,4);

    Serial.print("send result : ");
    Serial.println(wqd);
    delay(500);
  }
  delay(10);

  client.stop();
  Serial.println("Client Disconnected");

  delay(10);
}




void printWifiStatus() {
  // print the SSID of the network you're attached to
  Serial.print("SSID: ");
  Serial.println(WiFi.SSID());

  // print your WiFi shield's IP address
  IPAddress ip = WiFi.localIP();
  Serial.print("IP Address: ");
  Serial.println(ip);

  // print the received signal strength
  long rssi = WiFi.RSSI();
  Serial.print("Signal strength (RSSI):");
  Serial.print(rssi);
  Serial.println(" dBm");
}