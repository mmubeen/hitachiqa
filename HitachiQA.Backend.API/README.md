# HitachiQA Test Runner API

## Overview
The **HitachiQA Test Runner API** allows you to execute automated UI tests using Playwright or Selenium by sending test scenarios via HTTP requests. It also provides functionality to retrieve recorded test execution videos.

## Features
- Execute test scenarios using Playwright or Selenium.
- Send test execution requests via HTTP `POST` requests.
- Retrieve recorded execution videos via `GET` requests.

---

## **Installation & Setup**

either run from visual studio OR

1. **Clone the Repository**
   ```sh
   git clone https://tfs-hisol-crm.visualstudio.com/DefaultCollection/HitachiQA/_git/HitachiQA
   cd HitachiQA.Backend.API
   ```

2. **Build & Run the API**
   ```sh
   dotnet build
   dotnet run
   ```

3. **API is now available at**
   ```
   http://localhost:7111
   ```

---

## **API Endpoints**

### **1️⃣ Execute a Test Run**
#### **Request**
- **Method:** `POST`
- **Endpoint:** `/api/run`
- **Content-Type:** `application/json`
- **Body:**
  ```json
  {
    "driver": "Playwright",
    "browser": "chrome",
    "host": "https://www.yahoo.com/",
    "scenarios": [
      {
        "name": "Signup Test",
        "steps": [
          {
            "description": "Navigate to login page",
            "actions": [
              {
                "type": "Click",
                "selector": "Sign in"
              },
              {
                "type": "Click",
                "selector": "createacc"
              },
              {
                "type": "Setfieldvalue",
                "selector": "usernamereg-firstName",
                "value": "miguel"
              },
              {
                "type": "Setfieldvalue",
                "selector": "userId",
                "value": "userId"
              },
              {
                "type": "Setfieldvalue",
                "selector": "yid-domain-selector",
                "value": "myyahoo.com"
              },
              {
                "type": "Setfieldvalue",
                "selector": "usernamereg-password",
                "value": "Some Password"
              }
            ]
          }
        ]
      }
    ]
  }
  ```

#### **Response**
- **Status Code:** `200 OK`
- **Body:**
  ```json
  {
    "driver": "Playwright",
    "browser": "chrome",
    "host": "https://www.yahoo.com/",
    "scenarios": [ ... ]
  }
  ```

---

### **2️⃣ Retrieve Test Execution Video**
#### **Request**
- **Method:** `GET`
- **Endpoint:** `/api/video/{fileName}`

#### **Example**
```sh
curl -X GET "http://localhost:7111/api/video/test-video.webm"
```

#### **Response**
- **Content-Type:** `video/webm`
- **Opens in Chrome:**  
  ```
  http://localhost:7111/api/video/test-video.webm
  ```

---

### **3️⃣ Check API Status**
#### **Request**
- **Method:** `GET`
- **Endpoint:** `/api/run`
  
#### **Response**
```json
["Not implemented", "We'll eventually return you status/logs here"]
```

---

## **Run Tests**
To run the tests:
```sh
dotnet test
```

---

## **Notes**
- Ensure **Playwright** is installed before running tests:
  ```sh
  playwright install
  ```
- Videos are stored in the `/Videos/` directory and served via the `/api/video/{filename}` endpoint.

---

