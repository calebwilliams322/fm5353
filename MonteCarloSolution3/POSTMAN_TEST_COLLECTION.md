# MonteCarloAPI - Postman Test Collection

## Base URL
```
http://localhost:5262
```

---

## 1. OPTIONS CRUD ENDPOINTS

### 1.1 Create European Call Option
**POST** `/api/options`

```json
{
  "optionParameters": {
    "optionType": 0,
    "strike": 100.0,
    "isCall": true
  }
}
```

### 1.2 Create European Put Option
**POST** `/api/options`

```json
{
  "optionParameters": {
    "optionType": 0,
    "strike": 105.0,
    "isCall": false
  }
}
```

### 1.3 Create Asian Option (Arithmetic Average)
**POST** `/api/options`

```json
{
  "optionParameters": {
    "optionType": 1,
    "strike": 100.0,
    "isCall": true,
    "averagingType": 0,
    "observationFrequency": 1
  }
}
```

### 1.4 Create Asian Option (Geometric Average)
**POST** `/api/options`

```json
{
  "optionParameters": {
    "optionType": 1,
    "strike": 100.0,
    "isCall": false,
    "averagingType": 1,
    "observationFrequency": 1
  }
}
```

### 1.5 Create Digital/Binary Option
**POST** `/api/options`

```json
{
  "optionParameters": {
    "optionType": 2,
    "strike": 100.0,
    "isCall": true,
    "digitalCondition": 0
  }
}
```

### 1.6 Create Barrier Option (Up-and-Out)
**POST** `/api/options`

```json
{
  "optionParameters": {
    "optionType": 3,
    "strike": 100.0,
    "isCall": true,
    "barrierOptionType": 1,
    "barrierDir": 0,
    "barrierLevel": 120.0
  }
}
```

### 1.7 Create Barrier Option (Down-and-In)
**POST** `/api/options`

```json
{
  "optionParameters": {
    "optionType": 3,
    "strike": 100.0,
    "isCall": true,
    "barrierOptionType": 0,
    "barrierDir": 1,
    "barrierLevel": 80.0
  }
}
```

### 1.8 Create Lookback Option (Max)
**POST** `/api/options`

```json
{
  "optionParameters": {
    "optionType": 4,
    "strike": 100.0,
    "isCall": true,
    "lookbackOptionType": 0
  }
}
```

### 1.9 Create Lookback Option (Min)
**POST** `/api/options`

```json
{
  "optionParameters": {
    "optionType": 4,
    "strike": 100.0,
    "isCall": false,
    "lookbackOptionType": 1
  }
}
```

### 1.10 Create Range Option
**POST** `/api/options`

```json
{
  "optionParameters": {
    "optionType": 5,
    "strike": 100.0,
    "rangeObservationFrequency": 1
  }
}
```

### 1.11 Get All Options
**GET** `/api/options`

*No body required*

### 1.12 Get Option By ID
**GET** `/api/options/1`

*No body required*

### 1.13 Update Option
**PUT** `/api/options/1`

```json
{
  "optionParameters": {
    "optionType": 0,
    "strike": 110.0,
    "isCall": true
  }
}
```

### 1.14 Delete Option
**DELETE** `/api/options/1`

*No body required*

---

## 2. PRICING ENDPOINTS

### 2.1 Price Option - Plain Monte Carlo (Basic)
**POST** `/api/pricing/1`

```json
{
  "initialPrice": 100.0,
  "volatility": 0.2,
  "riskFreeRate": 0.05,
  "timeToExpiry": 1.0,
  "timeSteps": 252,
  "numberOfPaths": 10000,
  "useMultithreading": true,
  "simMode": 0,
  "referenceStrike": 100.0
}
```

### 2.2 Price Option - Antithetic Variates
**POST** `/api/pricing/1`

```json
{
  "initialPrice": 100.0,
  "volatility": 0.25,
  "riskFreeRate": 0.03,
  "timeToExpiry": 1.0,
  "timeSteps": 252,
  "numberOfPaths": 20000,
  "useMultithreading": true,
  "simMode": 1,
  "referenceStrike": 100.0
}
```

### 2.3 Price Option - Control Variates
**POST** `/api/pricing/1`

```json
{
  "initialPrice": 100.0,
  "volatility": 0.2,
  "riskFreeRate": 0.05,
  "timeToExpiry": 1.0,
  "timeSteps": 252,
  "numberOfPaths": 10000,
  "useMultithreading": true,
  "simMode": 2,
  "referenceStrike": 100.0
}
```

### 2.4 Price Option - Antithetic + Control Variates
**POST** `/api/pricing/1`

```json
{
  "initialPrice": 100.0,
  "volatility": 0.2,
  "riskFreeRate": 0.05,
  "timeToExpiry": 1.0,
  "timeSteps": 252,
  "numberOfPaths": 15000,
  "useMultithreading": true,
  "simMode": 3,
  "referenceStrike": 100.0
}
```

### 2.5 Price Option - Van Der Corput (Quasi-Random)
**POST** `/api/pricing/1`

```json
{
  "initialPrice": 100.0,
  "volatility": 0.2,
  "riskFreeRate": 0.05,
  "timeToExpiry": 1.0,
  "timeSteps": 252,
  "numberOfPaths": 1024,
  "useMultithreading": false,
  "simMode": 4,
  "referenceStrike": 100.0,
  "vdCBase1": 2,
  "vdCBase2": 5,
  "vdCPoints": 1024
}
```

### 2.6 Price Option - Short Maturity
**POST** `/api/pricing/1`

```json
{
  "initialPrice": 100.0,
  "volatility": 0.3,
  "riskFreeRate": 0.05,
  "timeToExpiry": 0.25,
  "timeSteps": 63,
  "numberOfPaths": 10000,
  "useMultithreading": true,
  "simMode": 0,
  "referenceStrike": 100.0
}
```

### 2.7 Price Option - Long Maturity
**POST** `/api/pricing/1`

```json
{
  "initialPrice": 100.0,
  "volatility": 0.2,
  "riskFreeRate": 0.05,
  "timeToExpiry": 5.0,
  "timeSteps": 1260,
  "numberOfPaths": 10000,
  "useMultithreading": true,
  "simMode": 0,
  "referenceStrike": 100.0
}
```

### 2.8 Price Option - High Volatility
**POST** `/api/pricing/1`

```json
{
  "initialPrice": 100.0,
  "volatility": 0.8,
  "riskFreeRate": 0.05,
  "timeToExpiry": 1.0,
  "timeSteps": 252,
  "numberOfPaths": 20000,
  "useMultithreading": true,
  "simMode": 1,
  "referenceStrike": 100.0
}
```

### 2.9 Price Option - Low Volatility
**POST** `/api/pricing/1`

```json
{
  "initialPrice": 100.0,
  "volatility": 0.1,
  "riskFreeRate": 0.05,
  "timeToExpiry": 1.0,
  "timeSteps": 252,
  "numberOfPaths": 10000,
  "useMultithreading": true,
  "simMode": 0,
  "referenceStrike": 100.0
}
```

### 2.10 Price Option - Deep In The Money
**POST** `/api/pricing/1`

```json
{
  "initialPrice": 150.0,
  "volatility": 0.2,
  "riskFreeRate": 0.05,
  "timeToExpiry": 1.0,
  "timeSteps": 252,
  "numberOfPaths": 10000,
  "useMultithreading": true,
  "simMode": 0,
  "referenceStrike": 100.0
}
```

### 2.11 Price Option - Deep Out of The Money
**POST** `/api/pricing/1`

```json
{
  "initialPrice": 50.0,
  "volatility": 0.2,
  "riskFreeRate": 0.05,
  "timeToExpiry": 1.0,
  "timeSteps": 252,
  "numberOfPaths": 10000,
  "useMultithreading": true,
  "simMode": 0,
  "referenceStrike": 100.0
}
```

---

## 3. ERROR TESTING

### 3.1 Invalid - Negative Volatility
**POST** `/api/pricing/1`

```json
{
  "initialPrice": 100.0,
  "volatility": -0.2,
  "riskFreeRate": 0.05,
  "timeToExpiry": 1.0,
  "timeSteps": 252,
  "numberOfPaths": 10000
}
```
*Expected: 400 Bad Request - "Volatility must be greater than 0."*

### 3.2 Invalid - Zero Paths
**POST** `/api/pricing/1`

```json
{
  "initialPrice": 100.0,
  "volatility": 0.2,
  "riskFreeRate": 0.05,
  "timeToExpiry": 1.0,
  "timeSteps": 252,
  "numberOfPaths": 0
}
```
*Expected: 400 Bad Request - "Number of paths must be greater than 0."*

### 3.3 Invalid - Too Many Paths
**POST** `/api/pricing/1`

```json
{
  "initialPrice": 100.0,
  "volatility": 0.2,
  "riskFreeRate": 0.05,
  "timeToExpiry": 1.0,
  "timeSteps": 252,
  "numberOfPaths": 2000000
}
```
*Expected: 400 Bad Request - "Number of paths cannot exceed 1,000,000"*

### 3.4 Invalid - Extreme Volatility
**POST** `/api/pricing/1`

```json
{
  "initialPrice": 100.0,
  "volatility": 10.0,
  "riskFreeRate": 0.05,
  "timeToExpiry": 1.0,
  "timeSteps": 252,
  "numberOfPaths": 10000
}
```
*Expected: 400 Bad Request - "Volatility cannot exceed 500%"*

### 3.5 Invalid - Zero Initial Price
**POST** `/api/pricing/1`

```json
{
  "initialPrice": 0,
  "volatility": 0.2,
  "riskFreeRate": 0.05,
  "timeToExpiry": 1.0,
  "timeSteps": 252,
  "numberOfPaths": 10000
}
```
*Expected: 400 Bad Request - "Initial price must be greater than 0."*

### 3.6 Invalid - Non-existent Option
**POST** `/api/pricing/999`

```json
{
  "initialPrice": 100.0,
  "volatility": 0.2,
  "riskFreeRate": 0.05,
  "timeToExpiry": 1.0,
  "timeSteps": 252,
  "numberOfPaths": 10000
}
```
*Expected: 404 Not Found - "Option with ID 999 not found."*

### 3.7 Invalid - Barrier Direction (Up barrier below spot)
First create barrier option:
**POST** `/api/options`
```json
{
  "optionParameters": {
    "optionType": 3,
    "strike": 100.0,
    "isCall": true,
    "barrierOptionType": 1,
    "barrierDir": 0,
    "barrierLevel": 90.0
  }
}
```

Then price with initial price above barrier:
**POST** `/api/pricing/{id}`
```json
{
  "initialPrice": 100.0,
  "volatility": 0.2,
  "riskFreeRate": 0.05,
  "timeToExpiry": 1.0,
  "timeSteps": 252,
  "numberOfPaths": 10000
}
```
*Expected: 400 Bad Request - "Up barrier must be above initial price."*

---

## ENUM REFERENCE

### OptionType
- `0` = European
- `1` = Asian
- `2` = Digital
- `3` = Barrier
- `4` = Lookback
- `5` = Range

### SimulationMode
- `0` = Plain
- `1` = Antithetic
- `2` = ControlVariate
- `3` = AntitheticAndControlVariate
- `4` = VanDerCorput

### AveragingType (Asian options)
- `0` = Arithmetic
- `1` = Geometric

### BarrierType
- `0` = KnockIn
- `1` = KnockOut

### BarrierDirection
- `0` = Up
- `1` = Down

### LookbackType
- `0` = Max
- `1` = Min

### ConditionType (Digital options)
- `0` = AboveStrike
- `1` = BelowStrike
