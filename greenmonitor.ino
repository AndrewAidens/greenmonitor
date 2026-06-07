#include <WiFi.h>
#include <HTTPClient.h>

// WiFi settings
const char* ssid = "Aidens33";
const char* password = "123698741";

// Server
const char* serverUrl = "http://192.168.0.106:5071/api/sensor/readings";
const char* apiKey = "arduino-secret-key";

// Pins
const int TMP36_PIN = 1;   // GPIO1
const int SOIL_PIN = 2;    // GPIO2

void setup() {
  delay(3000);
  Serial.begin(115200);
  pinMode(LED_BUILTIN, OUTPUT);
  for (int i = 0; i < 3; i++) {
    digitalWrite(LED_BUILTIN, HIGH);
    delay(200);
    digitalWrite(LED_BUILTIN, LOW);
    delay(200);
  }
  
  WiFi.begin(ssid, password);
Serial.print("Підключення до WiFi");
  while (WiFi.status() != WL_CONNECTED) {
  delay(500);
  Serial.print(".");
  }
Serial.println("\nПідключено!");
}

void loop() {
  // get temperature from TMP36
  Serial.print("WiFi статус: ");
  Serial.println(WiFi.status());
  int rawTemp = analogRead(TMP36_PIN);
  float voltage = rawTemp * (3.3 / 4095.0);
  float temperature = (voltage - 0.5) * 100.0 + 11.0;

  // get moisture
  int rawSoil = analogRead(SOIL_PIN);
  // convert into percent (4095 = сухо, 0 = мокро)
  float humidity = map(rawSoil, 4095, 0, 0, 100); // tut koment
  //float temperature = 20.0;
  //float humidity = 50.0;

  Serial.print("Температура: ");
  Serial.print(temperature);
  Serial.print("°C | Вологість: ");
  Serial.print(humidity);
  Serial.println("%");

  // send to the server
  if (WiFi.status() == WL_CONNECTED) {
    Serial.println("Відправляємо на сервер...");
    HTTPClient http;
    http.begin(serverUrl);
    http.addHeader("Content-Type", "application/json");
    http.addHeader("X-Api-Key", apiKey);

    String body = "{\"temperature\":" + String(temperature, 1) + 
                  ",\"humidity\":" + String(humidity, 1) + "}";
    
    int responseCode = http.POST(body);
    Serial.print("Відповідь сервера: ");
    Serial.println(responseCode);
    if (responseCode < 0) {
    Serial.print("Помилка: ");
    Serial.println(http.errorToString(responseCode));
    }
    http.end();
  }
  if (WiFi.status() != WL_CONNECTED) {
    Serial.println("WiFi відключився, перепідключення...");
    WiFi.begin(ssid, password);
    delay(5000);
    return;
  }

  // wait an hour
  delay(3600000);
}