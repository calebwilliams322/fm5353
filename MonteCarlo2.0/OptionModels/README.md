# 🧭 OptionModels Directory — MonteCarloOptionPricer

This directory defines the **financial model layer** of the Monte Carlo Option Pricer.  
Each class in this folder represents a *financial instrument* (e.g., European, Asian, Barrier options), implementing common behaviors via a shared interface.

---

## Contents

| File | Description |
|------|--------------|
| **`IOption.cs`** | Defines the core interface that all option types must implement. It establishes a consistent pricing and Greek-computation API. |
| **`EuropeanOption.cs`** | Implements a vanilla European call/put option using Monte Carlo simulation and finite-difference Greeks. |

---

## Overview of Design Philosophy

### Layered Architecture

| Layer | Responsibility |
|--------|----------------|
| `OptionModels/` | Defines *what* an option is and how it should be priced. |
| `Simulation/` | Implements *how* the underlying stochastic process (GBM) is simulated. |
| `Models/` | Holds shared configuration and data containers (`SimulationParameters`, `PricingResult`). |

The `OptionModels` layer **never re-implements simulation logic**.  
Each option delegates pricing and path generation to the shared `MonteCarloSimulator`.

---

## Interface: `IOption`

```csharp
namespace MonteCarloOptionPricer.OptionModels
{
    public interface IOption
    {
        double Strike { get; set; }
        DateTime Expiry { get; set; }
        bool IsCall { get; set; }

        PricingResult GetPrice(
            double volatility,
            double riskFreeRate,
            int timeSteps,
            int numberOfPaths,
            bool calculateGreeks,
            bool useMultithreading = true,
            SimulationMode simMode = SimulationMode.Plain
        );
    }
}


## 🇪🇺 EuropeanOption

### Overview

`EuropeanOption` represents the most fundamental derivative — a **vanilla European call or put**.  
Its payoff depends only on the **underlying price at expiry** \( S_T \), not on the path the price took to get there.  
This class provides the foundation for all other option types in the framework.

It implements the shared `IOption` interface and integrates seamlessly with the `MonteCarloSimulator` to estimate option values and Greeks under the risk-neutral measure.

### Class Summary

| Property | Type | Description |
|-----------|------|-------------|
| `InitialPrice` | `double` | Underlying price \( S_0 \). |
| `Strike` | `double` | Strike price \( K \). |
| `Expiry` | `DateTime` | Expiration date. |
| `IsCall` | `bool` | `true` for a call, `false` for a put. |

### Payoff Definition

| Type | Payoff Formula |
|------|----------------|
| **Call** | \( \max(S_T - K, 0) \) |
| **Put** | \( \max(K - S_T, 0) \) |

The EuropeanOption payoff depends **only on the terminal price** \( S_T \), making it the simplest and most direct benchmark for validating simulation accuracy and convergence.

### Pricing Logic

1. **Simulation Setup:**  
   A `SimulationParameters` object is configured with all relevant inputs:
   \[
   S_0, \ \sigma, \ r, \ T, \ N_{\text{steps}}, \ N_{\text{paths}}, \text{mode}
   \]
   and passed to the `MonteCarloSimulator`.

2. **Monte Carlo Simulation:**  
   The simulator generates terminal prices under a **geometric Brownian motion (GBM)** model:
   \[
   S_T = S_0 \exp\left( (r - \tfrac{1}{2}\sigma^2)T + \sigma \sqrt{T} Z \right)
   \]
   where \( Z \sim \mathcal{N}(0,1) \).

3. **Payoff Evaluation:**  
   For each simulated \( S_T \), compute the payoff:
   - Call: \( \max(S_T - K, 0) \)
   - Put: \( \max(K - S_T, 0) \)

4. **Discounting:**  
   The average discounted payoff gives the price estimate:
   \[
   V_0 = e^{-rT} \cdot \mathbb{E}^{\mathbb{Q}}[\text{Payoff}]
   \]

5. **Standard Error:**  
   The Monte Carlo standard error is reported (except in quasi-random modes like Van der Corput).

### Greeks Estimation

Greeks are computed using **finite difference bump-and-revalue** techniques, with a *fixed random seed* for variance reduction:

| Greek | Meaning | Computation |
|--------|----------|-------------|
| **Δ (Delta)** | Price sensitivity to \( S_0 \) | \( \frac{V(S_0 + \epsilon) - V(S_0)}{\epsilon} \) |
| **Γ (Gamma)** | Curvature w.r.t \( S_0 \) | \( \frac{V(S_0 + \epsilon) - 2V(S_0) + V(S_0 - \epsilon)}{\epsilon^2} \) |
| **ν (Vega)** | Sensitivity to volatility | \( \frac{V(\sigma + \epsilon) - V(\sigma)}{\epsilon} \) |
| **ρ (Rho)** | Sensitivity to interest rate | \( \frac{V(r + \epsilon) - V(r)}{\epsilon} \) |
| **θ (Theta)** | Sensitivity to time decay | \( \frac{V(T - \epsilon) - V(T)}{\epsilon} \) |

Each revaluation call reuses the same random seed to ensure consistent stochastic noise cancellation between bumps.



## AsianOption

### Overview

`AsianOption` represents an **average-price (Asian) derivative**, where the payoff depends on the *average underlying price* observed throughout the option’s lifetime.  
It extends `IOption` and uses the same Monte Carlo simulation framework as `EuropeanOption`, but differs in how it computes its terminal payoff.

Asian options reduce volatility exposure because averaging smooths out price fluctuations.  
This makes them cheaper than otherwise-equivalent European options.

### Class Summary

| Property | Type | Description |
|-----------|------|-------------|
| `InitialPrice` | `double` | Starting underlying price \( S_0 \). |
| `Strike` | `double` | Strike price \( K \). |
| `Expiry` | `DateTime` | Expiration date. |
| `IsCall` | `bool` | `true` for call, `false` for put. |
| `AveragingType` | `AveragingType` | Determines whether the average is **Arithmetic** or **Geometric**. |

>  `ObservationFrequency` is no longer needed — the option samples **every step** in the simulation path.

### Payoff Definition

| Averaging Type | Call Payoff | Put Payoff |
|----------------|--------------|-------------|
| **Arithmetic** | \( \max(\bar{S}_{\text{arith}} - K, 0) \) | \( \max(K - \bar{S}_{\text{arith}}, 0) \) |
| **Geometric** | \( \max(\bar{S}_{\text{geom}} - K, 0) \) | \( \max(K - \bar{S}_{\text{geom}}, 0) \) |

Where:

- \( \bar{S}_{\text{arith}} = \frac{1}{N} \sum_{i=1}^{N} S_i \)
- \( \bar{S}_{\text{geom}} = \left( \prod_{i=1}^{N} S_i \right)^{1/N} \)

### Key Implementation Notes

- Requires **`keepPaths: true`** in the simulator to compute the averages.
- Greeks are estimated via **finite differences**, reusing the same random seeds for stability.
- The averaging dampens variance, so the Monte Carlo estimator converges faster.
- Extends easily to Asian barrier or fixed-strike variants by swapping the payoff function.

---

## DigitalOption

### Overview

`DigitalOption` models **binary (digital)** options — derivatives that pay a *fixed amount* or the *underlying asset* if a certain condition is met at expiry.  
This class supports both **cash-or-nothing** and **asset-or-nothing** structures, for calls and puts.

### Class Summary

| Property | Type | Description |
|-----------|------|-------------|
| `InitialPrice` | `double` | Underlying starting price \( S_0 \). |
| `Strike` | `double` | Strike price \( K \). |
| `Expiry` | `DateTime` | Option expiry date. |
| `IsCall` | `bool` | `true` for call, `false` for put. |
| `IsCashOrNothing` | `bool` | `true` → pays a fixed cash amount, `false` → pays the asset. |
| `Payout` | `double` | Fixed cash payout amount (default = 1.0). |

### Payoff Definition

| Type | Condition | Payoff |
|------|------------|--------|
| **Cash-or-Nothing Call** | \( S_T > K \) | \( \text{Payout} \) |
| **Cash-or-Nothing Put** | \( S_T < K \) | \( \text{Payout} \) |
| **Asset-or-Nothing Call** | \( S_T > K \) | \( S_T \) |
| **Asset-or-Nothing Put** | \( S_T < K \) | \( S_T \) |

### Behavior and Intuition

- **Cash-or-Nothing** options behave like probabilistic bets: their value roughly tracks the risk-neutral probability of finishing in-the-money.
- **Asset-or-Nothing** options embed exposure to the *magnitude* of \( S_T \) as well as its probability.
- Prices are always smaller than equivalent European options because the payout is binary or capped.

### Implementation Notes

- Uses the same `MonteCarloSimulator` for path generation.
- Payoff function is vectorized for efficiency and supports `Antithetic` pairing.
- Control variates improve convergence for cash-style digitals since payoff discontinuity increases variance.
- Greeks computed via **bump-and-revalue** with fixed random seed to reduce noise.

---

## Common Design Traits (All OptionModels)

| Feature | Description |
|----------|--------------|
| **Implements `IOption`** | Enforces consistent pricing API (`GetPrice(...)`). |
| **Monte Carlo Integration** | Delegates simulation to the shared `MonteCarloSimulator`. |
| **Reproducible Greeks** | Uses identical RNG seeds across bumps. |
| **Modular Payoffs** | Each option type defines only its unique payoff logic. |
| **Extensible Framework** | New exotic options (barrier, lookback, range, etc.) can be added by reusing this structure. |

---


## BarrierOption

### Overview

`BarrierOption` represents a **path-dependent derivative** whose payoff depends on whether the underlying price *touches a specified barrier* during the option’s lifetime.  
It extends `IOption` and leverages the same Monte Carlo simulation engine as the other option types but requires **`keepPaths: true`** so that barrier crossings can be detected along the simulated price paths.

Barrier options are commonly used in exotic structures to create cheaper or more tailored exposures than standard European options.

### Class Summary

| Property | Type | Description |
|-----------|------|-------------|
| `InitialPrice` | `double` | Starting underlying price \( S_0 \). |
| `Strike` | `double` | Strike price \( K \). |
| `Expiry` | `DateTime` | Expiration date. |
| `IsCall` | `bool` | `true` for call, `false` for put. |
| `BarrierOptionType` | `BarrierType` | `KnockIn` or `KnockOut` — determines if crossing activates or cancels the option. |
| `BarrierDir` | `BarrierDirection` | `Up` or `Down` — defines whether the barrier triggers on upward or downward crossings. |
| `BarrierLevel` | `double` | The barrier price level \( B \). |

### Payoff Definition

| Barrier Type | Direction | Call Payoff | Put Payoff |
|---------------|------------|--------------|-------------|
| **Knock-Out** | **Up** | \( 0 \) if \( \max(S_t) \ge B \); otherwise \( \max(S_T - K, 0) \) | \( 0 \) if \( \max(S_t) \ge B \); otherwise \( \max(K - S_T, 0) \) |
| **Knock-Out** | **Down** | \( 0 \) if \( \min(S_t) \le B \); otherwise \( \max(S_T - K, 0) \) | \( 0 \) if \( \min(S_t) \le B \); otherwise \( \max(K - S_T, 0) \) |
| **Knock-In** | **Up** | \( \max(S_T - K, 0) \) if \( \max(S_t) \ge B \); otherwise \( 0 \) | \( \max(K - S_T, 0) \) if \( \max(S_t) \ge B \); otherwise \( 0 \) |
| **Knock-In** | **Down** | \( \max(S_T - K, 0) \) if \( \min(S_t) \le B \); otherwise \( 0 \) | \( \max(K - S_T, 0) \) if \( \min(S_t) \le B \); otherwise \( 0 \) |

### Key Implementation Notes

- Requires **full path storage** (`keepPaths: true`) to track min/max price movements.  
- Barrier logic is evaluated by scanning each simulated path for breaches relative to the barrier level.  
- All Greeks (Δ, Γ, ν, θ, ρ) are computed using **finite differences**, reusing identical random seeds to minimize Monte Carlo noise.  
- Satisfies the **barrier parity condition**:

  \[
  \text{Knock-In} + \text{Knock-Out} \approx \text{European Option}
  \]


---

## LookbackOption

### Overview

`LookbackOption` represents a **path-dependent exotic derivative** whose payoff depends on the **maximum or minimum underlying price** observed during the option’s lifetime.  
Unlike European or Asian options that only consider the terminal or averaged price, lookback options “look back” at the *entire price path* to determine the best (or worst) price achieved.

This implementation focuses on the **fixed-strike lookback** variant, where the strike price \( K \) is fixed and the payoff depends on the observed extrema.  
It extends `IOption` and leverages the same Monte Carlo simulation engine as other option types, requiring `keepPaths: true` to access all simulated price steps.

Lookback options provide **insurance against poor timing**, making them valuable in volatile markets but more expensive than standard European options.

---

### Class Summary

| Property | Type | Description |
|-----------|------|-------------|
| `InitialPrice` | `double` | Starting underlying price \( S_0 \). |
| `Strike` | `double` | Fixed strike price \( K \). |
| `Expiry` | `DateTime` | Expiration date of the option. |
| `IsCall` | `bool` | `true` for call, `false` for put. |
| `LookbackOptionType` | `LookbackType` | Specifies whether to use the **maximum** or **minimum** price path for payoff (default = `Max`). |

> The `LookbackType` enum allows extensions to floating-strike lookbacks in the future (e.g., \( C = (S_T - \min S_t)_+ \)).

---

### Payoff Definition

| Type | Call Payoff | Put Payoff |
|------|--------------|-------------|
| **Fixed-Strike Lookback** | \( C = \max(\max(S_t) - K, 0) \) | \( P = \max(K - \min(S_t), 0) \) |

Where:
- \( \max(S_t) \) is the **highest** simulated price observed along the path.
- \( \min(S_t) \) is the **lowest** simulated price observed along the path.
- The payoff reflects the *best-case* scenario for the holder within the option’s lifetime.

---

## RangeOption

### Overview

`RangeOption` represents a **path-dependent long-volatility derivative**.  
Its payoff depends on the *difference between the highest and lowest price* of the underlying asset during the option’s life.  
Unlike European or Asian options, it has **no directional exposure** — it purely measures the realized *range* of the underlying.

This option benefits from large price fluctuations in either direction and is therefore considered a **volatility-driven** product.

### Class Summary

| Property | Type | Description |
|-----------|------|-------------|
| `InitialPrice` | `double` | Starting underlying price \( S_0 \). |
| `Strike` | `double` | Placeholder for interface compliance (not used). |
| `Expiry` | `DateTime` | Option expiration date. |
| `IsCall` | `bool` | Placeholder (always true; not used). |

### Payoff Definition

| Option Type | Payoff Formula |
|--------------|----------------|
| **Range Option** | \( \text{Payoff} = \max(S_t) - \min(S_t) \) |

- The option pays out the *difference* between the maximum and minimum prices observed during the simulation.
- It is **always positive** and **non-directional** — higher volatility leads to larger ranges and thus higher value.

### Key Implementation Notes

- Requires **`keepPaths: true`** in the simulator to access full path data.
- The payoff is computed for each simulated path via:
  \[
  \text{Payoff}_i = \max(S_{t, i}) - \min(S_{t, i})
  \]
- The resulting discounted average is used to estimate the price:
  \[
  P = e^{-rT} \cdot \frac{1}{N} \sum_i \text{Payoff}_i
  \]
- All Greeks (Δ, Γ, ν, θ, ρ) are estimated using **finite differences** with re-seeded simulations for consistency.