# MonteCarloSimulator Class

## Purpose
The `MonteCarloSimulator` class provides a flexible framework for simulating
asset prices under geometric Brownian motion. It supports multiple variance
reduction methods including **antithetic sampling**, **control variates**, and
**low-discrepancy (Van der Corput)** sequences.

## Key Methods

### `Simulate(SimulationParameters p, bool keepPaths = false)`
Runs the full simulation pipeline and returns a `SimulationOutput` object
containing terminal prices, optional paths, and (for CV modes) hedge PnL.

**Parameters**
- `p`: `SimulationParameters` — configuration of the simulation.
- `keepPaths`: whether to store the full time-series for each path.

**Returns**
- `SimulationOutput` with:
  - `Terminals`: final simulated prices
  - `Paths`: optional price matrices
  - `HedgePnL`: optional hedge results for Control Variate runs

### `SimulateTerminals(SimulationParameters p)`
Convenience wrapper — returns only terminal prices.

### `SimulatePaths(SimulationParameters p)`
Convenience wrapper — returns only paths.

---

## Supported Modes
| Mode | Description |
|------|--------------|
| `Plain` | Standard GBM paths |
| `Antithetic` | Uses z and -z to reduce variance |
| `VanDerCorput` | Uses quasi-random sequences |
| `ControlVariate` | Includes hedging-based control variate |
| `AntitheticAndControlVariate` | Combines both variance reductions |

---

## Example Usage

```csharp
var p = new SimulationParameters
{
    InitialPrice = 100,
    Volatility = 0.2,
    RiskFreeRate = 0.05,
    TimeToExpiry = 1.0,
    TimeSteps = 252,
    NumberOfPaths = 10000,
    SimMode = SimulationMode.ControlVariate,
    UseMultithreading = true,
    ReferenceStrike = 100
};

var result = MonteCarloSimulator.Simulate(p, keepPaths: true);
Console.WriteLine($"Mean terminal price: {result.Terminals.Average():F4}");
