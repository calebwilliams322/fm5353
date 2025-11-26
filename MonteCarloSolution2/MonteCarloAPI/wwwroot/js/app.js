// API base URL
const API_BASE = '';

// Global state
let currentPortfolioId = null;
let portfolios = [];
let options = [];
let stocks = [];
let exchanges = [];
let pnlChart = null;
let currentTimePeriod = '1m';
let portfolioLoadedOnce = false;

// ==================== Initialization ====================

document.addEventListener('DOMContentLoaded', function() {
    initializeNavigation();
    initializeModals();
    initializeInstructionsModal();
    initializeForms();
    initializeMarketParamsModal();
    initializePricingResultModal();
    initializePnLChart();
    loadInitialData();
});

// ==================== Navigation ====================

function initializeNavigation() {
    const navButtons = document.querySelectorAll('.nav-btn');

    navButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            const viewName = btn.dataset.view;
            switchView(viewName);

            // Update active nav button
            navButtons.forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
        });
    });
}

function switchView(viewName) {
    // Hide all views
    document.querySelectorAll('.view').forEach(view => {
        view.classList.remove('active');
    });

    // Show selected view
    const targetView = document.getElementById(`${viewName}-view`);
    if (targetView) {
        targetView.classList.add('active');

        // Load data for specific views
        switch(viewName) {
            case 'home':
                loadDashboard();
                break;
            case 'view-exchanges':
                loadExchangesTable();
                break;
            case 'view-stocks':
                loadStocksUniverseTable();
                break;
            case 'create-option':
                loadOptionsTable();
                break;
            case 'input-trades':
                loadTradesView();
                break;
            case 'view-positions':
                loadPositionsView();
                break;
            case 'view-portfolio':
                loadPortfoliosTable();
                break;
            case 'pricing-history':
                loadPricingHistoryTable();
                break;
        }
    }
}

// ==================== Modal Management ====================

function initializeModals() {
    const modal = document.getElementById('create-portfolio-modal');
    const openBtn = document.getElementById('create-portfolio-btn');
    const closeBtn = modal.querySelector('.modal-close');

    openBtn.addEventListener('click', () => {
        modal.classList.add('show');
    });

    closeBtn.addEventListener('click', () => {
        modal.classList.remove('show');
    });

    // Close on outside click
    modal.addEventListener('click', (e) => {
        if (e.target === modal) {
            modal.classList.remove('show');
        }
    });
}

function initializeInstructionsModal() {
    const modal = document.getElementById('instructions-modal');
    const openBtn = document.getElementById('instructions-btn');
    const closeBtn = document.getElementById('close-instructions-modal');

    openBtn.addEventListener('click', () => {
        modal.classList.add('show');
    });

    closeBtn.addEventListener('click', () => {
        modal.classList.remove('show');
    });

    // Close on outside click
    modal.addEventListener('click', (e) => {
        if (e.target === modal) {
            modal.classList.remove('show');
        }
    });
}

function showLoadingModal() {
    const modal = document.getElementById('loading-modal');
    modal.classList.add('show');
}

function hideLoadingModal() {
    const modal = document.getElementById('loading-modal');
    modal.classList.remove('show');
}

// ==================== Form Handlers ====================

function initializeForms() {
    // Create Portfolio Form
    document.getElementById('create-portfolio-form').addEventListener('submit', handleCreatePortfolio);

    // Create Exchange Form
    const exchangeForm = document.getElementById('create-exchange-form');
    if (exchangeForm) {
        exchangeForm.addEventListener('submit', handleCreateExchange);
    }

    // Create Option Form
    document.getElementById('create-option-form').addEventListener('submit', handleCreateOption);

    // Record Trade Form
    document.getElementById('record-trade-form').addEventListener('submit', handleRecordTrade);

    // Portfolio selection for dashboard
    document.getElementById('selected-portfolio').addEventListener('change', function() {
        currentPortfolioId = this.value ? parseInt(this.value) : null;
        // Reset the loaded flag when portfolio changes
        portfolioLoadedOnce = false;
        if (currentPortfolioId) {
            loadDashboard();
        } else {
            resetDashboard();
        }
    });

    // Update P&L button - show market params modal
    document.getElementById('update-pnl-btn').addEventListener('click', function() {
        if (!currentPortfolioId) {
            showError('Please select a portfolio first');
            return;
        }
        // Ensure modal is initialized
        initializeMarketParamsModal();
        // Set pending action to price the portfolio with user-specified params
        pendingPricingAction = async function(marketParams) {
            await loadPortfolioValuationWithParams(currentPortfolioId, marketParams);
        };
        // Show the modal using classList (consistent with rest of codebase)
        document.getElementById('market-params-modal').classList.add('show');
    });

    // Portfolio selection for positions view
    document.getElementById('positions-portfolio').addEventListener('change', function() {
        const portfolioId = this.value ? parseInt(this.value) : null;
        if (portfolioId) {
            loadPositions(portfolioId);
        }
    });

    // Price trade option button
    document.getElementById('price-trade-btn').addEventListener('click', priceTradeOption);

    // Price all positions button
    document.getElementById('price-all-positions-btn').addEventListener('click', priceAllPositions);

    // Price portfolio button
    document.getElementById('price-portfolio-btn').addEventListener('click', priceSelectedPortfolio);
}

async function handleCreatePortfolio(e) {
    e.preventDefault();

    const name = document.getElementById('portfolio-name').value;
    const description = document.getElementById('portfolio-description').value;
    const initialCash = parseFloat(document.getElementById('initial-cash').value);

    try {
        const response = await fetch(`${API_BASE}/api/portfolio`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ name, description, initialCash })
        });

        if (!response.ok) throw new Error('Failed to create portfolio');

        const portfolio = await response.json();

        // Close modal and reset form
        document.getElementById('create-portfolio-modal').classList.remove('show');
        e.target.reset();

        // Reload portfolios
        await loadPortfolios();

        // Select the new portfolio
        document.getElementById('selected-portfolio').value = portfolio.id;
        currentPortfolioId = portfolio.id;

        showSuccess('Portfolio created successfully!');
        loadDashboard();

    } catch (error) {
        showError('Error creating portfolio: ' + error.message);
    }
}

async function handleCreateOption(e) {
    e.preventDefault();

    const stockId = parseInt(document.getElementById('option-stock').value);
    const optionType = parseInt(document.getElementById('option-type').value);
    const isCall = document.getElementById('is-call').value === 'true';
    const strike = parseFloat(document.getElementById('strike').value);
    const expiryDateInput = document.getElementById('expiry-date').value;
    const barrier = parseFloat(document.getElementById('barrier').value);

    if (!stockId) {
        showError('Please select an underlying stock');
        return;
    }

    if (!expiryDateInput) {
        showError('Please select an expiry date');
        return;
    }

    // Convert date to ISO format with time
    const expiryDate = new Date(expiryDateInput + 'T00:00:00Z').toISOString();

    try {
        const response = await fetch(`${API_BASE}/api/options`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                stockId,
                optionParameters: {
                    optionType,
                    isCall,
                    strike,
                    expiryDate,
                    barrierLevel: barrier
                }
            })
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Failed to create option');
        }

        e.target.reset();
        showSuccess('Option created successfully!');
        await loadOptionsTable();
        await loadOptions(); // Refresh options list

    } catch (error) {
        showError('Error creating option: ' + error.message);
    }
}

async function handleRecordTrade(e) {
    e.preventDefault();

    const portfolioId = parseInt(document.getElementById('trade-portfolio').value);
    const assetType = parseInt(document.getElementById('trade-asset-type').value);
    const tradeType = parseInt(document.getElementById('trade-type').value);
    const quantity = parseInt(document.getElementById('quantity').value);
    const price = parseFloat(document.getElementById('price').value);
    const notes = document.getElementById('notes').value;

    // Build the trade payload based on asset type
    const tradePayload = {
        assetType,
        tradeType,
        quantity,
        price,
        notes
    };

    if (assetType === 0) {
        // Stock trade
        const stockId = document.getElementById('trade-stock').value;
        if (!stockId) {
            showError('Please select a stock');
            return;
        }
        tradePayload.stockId = parseInt(stockId);
    } else {
        // Option trade
        const optionId = document.getElementById('trade-option').value;
        if (!optionId) {
            showError('Please select an option');
            return;
        }
        tradePayload.optionId = parseInt(optionId);
    }

    try {
        const response = await fetch(`${API_BASE}/api/portfolio/${portfolioId}/trades`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(tradePayload)
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Failed to record trade');
        }

        e.target.reset();
        // Reset asset type to Option (default)
        document.getElementById('trade-asset-type').value = '1';
        handleAssetTypeChange();
        showSuccess('Trade recorded successfully!');
        await loadTradesTable(portfolioId);

    } catch (error) {
        showError('Error recording trade: ' + error.message);
    }
}

// ==================== Data Loading ====================

async function loadInitialData() {
    await updateStockPrices();
    await loadStocks();
    await loadPortfolios();
    await loadOptions();
    loadDashboard();
}

async function updateStockPrices() {
    try {
        console.log('Updating stock prices from Alpaca...');
        const response = await fetch(`${API_BASE}/api/stock/update-prices`, {
            method: 'POST'
        });
        const result = await response.json();
        console.log('Stock prices updated:', result.message);
    } catch (error) {
        console.error('Error updating stock prices:', error);
    }
}

async function loadPortfolios() {
    try {
        const response = await fetch(`${API_BASE}/api/portfolio`);
        portfolios = await response.json();

        // Update all portfolio selectors
        updatePortfolioSelectors();

    } catch (error) {
        console.error('Error loading portfolios:', error);
    }
}

function updatePortfolioSelectors() {
    const selectors = [
        document.getElementById('selected-portfolio'),
        document.getElementById('trade-portfolio'),
        document.getElementById('positions-portfolio')
    ];

    selectors.forEach(selector => {
        const currentValue = selector.value;
        selector.innerHTML = '<option value="">-- Select Portfolio --</option>';

        portfolios.forEach(portfolio => {
            const option = document.createElement('option');
            option.value = portfolio.id;
            option.textContent = `${portfolio.name} ($${portfolio.cash.toFixed(2)})`;
            selector.appendChild(option);
        });

        // Restore previous selection if it still exists
        if (currentValue && portfolios.some(p => p.id == currentValue)) {
            selector.value = currentValue;
        }
    });
}

async function loadStocks() {
    try {
        const response = await fetch(`${API_BASE}/api/stock`);
        stocks = await response.json();

        // Update all stock selectors
        updateStockSelectors();

    } catch (error) {
        console.error('Error loading stocks:', error);
    }
}

function updateStockSelectors() {
    const selectors = [
        document.getElementById('option-stock'),
        document.getElementById('trade-stock')
    ];

    selectors.forEach(selector => {
        if (!selector) return;

        const currentValue = selector.value;
        selector.innerHTML = '<option value="">-- Select Stock --</option>';

        stocks.forEach(stock => {
            const option = document.createElement('option');
            option.value = stock.id;
            option.textContent = `${stock.ticker} - ${stock.name} ($${stock.currentPrice.toFixed(2)})`;
            selector.appendChild(option);
        });

        // Restore previous selection if it still exists
        if (currentValue && stocks.some(s => s.id == currentValue)) {
            selector.value = currentValue;
        }
    });
}

async function loadOptions() {
    try {
        const response = await fetch(`${API_BASE}/api/options`);
        const data = await response.json();
        options = data.options || [];

        // Update option selector in trade form
        const optionSelector = document.getElementById('trade-option');
        optionSelector.innerHTML = '<option value="">-- Select Option --</option>';

        options.forEach(option => {
            const optionElement = document.createElement('option');
            const optionTypeNames = ['European', 'Asian', 'Digital', 'Barrier', 'Lookback', 'Range'];
            const typeName = optionTypeNames[option.optionParameters.optionType];
            const callPut = option.optionParameters.isCall ? 'Call' : 'Put';
            const ticker = option.stock?.ticker || 'N/A';

            optionElement.value = option.id;
            optionElement.textContent = `#${option.id} - ${typeName} ${callPut} @ ${option.optionParameters.strike} [${ticker}]`;
            optionSelector.appendChild(optionElement);
        });

    } catch (error) {
        console.error('Error loading options:', error);
    }
}

// ==================== Dashboard View ====================

async function loadDashboard() {
    if (!currentPortfolioId) {
        resetDashboard();
        portfolioLoadedOnce = false;
        return;
    }

    // Only automatically price portfolio on the FIRST load
    if (!portfolioLoadedOnce) {
        await loadPortfolioValuation(currentPortfolioId);
        portfolioLoadedOnce = true;
    } else {
        // On subsequent loads, just load the basic summary without full pricing
        await loadPortfolioSummaryOnly(currentPortfolioId);
    }
}

function resetDashboard() {
    document.getElementById('total-pnl').textContent = '--';
    document.getElementById('pnl-percentage').textContent = '--';
    document.getElementById('total-value').textContent = '--';
    document.getElementById('cash-balance').textContent = '--';
    document.getElementById('position-value').textContent = '--';
    document.getElementById('stock-pnl').textContent = '--';
    document.getElementById('stock-pnl-percentage').textContent = '--';
    document.getElementById('stock-value').textContent = '--';
    document.getElementById('option-pnl').textContent = '--';
    document.getElementById('option-pnl-percentage').textContent = '--';
    document.getElementById('option-value').textContent = '--';
    document.getElementById('portfolio-greeks').style.display = 'none';
}

// No longer used - kept for backwards compatibility
function manuallyUpdatePnL() {
    // Now handled via Update P&L button event listener
}

async function loadPortfolioSummaryOnly(portfolioId) {
    try {
        // Get portfolio summary without full pricing
        const summaryResponse = await fetch(`${API_BASE}/api/portfolio/${portfolioId}/summary`);
        const summary = await summaryResponse.json();

        // Update only the cash balance - keep previous P&L values
        document.getElementById('cash-balance').textContent = `$${summary.cash.toFixed(2)}`;

        // Update P&L chart with existing data
        updatePnLChart();

    } catch (error) {
        console.error('Error loading portfolio summary:', error);
        showError('Error loading portfolio data');
    }
}

async function manuallyUpdatePnL() {
    if (!currentPortfolioId) {
        showError('Please select a portfolio first');
        return;
    }
    await loadPortfolioValuation(currentPortfolioId);
}

async function loadPortfolioValuation(portfolioId) {
    try {
        // Show loading modal
        showLoadingModal();

        // Get portfolio summary first
        const summaryResponse = await fetch(`${API_BASE}/api/portfolio/${portfolioId}/summary`);
        const summary = await summaryResponse.json();

        // Update cash balance
        document.getElementById('cash-balance').textContent = `$${summary.cash.toFixed(2)}`;

        // Try to get valuation (requires market parameters)
        // Note: riskFreeRate and timeToExpiry are auto-calculated per option
        const valuationParams = {
            volatility: 0.2,
            timeSteps: 252,
            numberOfPaths: 10000,
            useMultithreading: true,
            simMode: 0
        };

        const valuationResponse = await fetch(`${API_BASE}/api/portfolio/${portfolioId}/value`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(valuationParams)
        });

        if (valuationResponse.ok) {
            const valuation = await valuationResponse.json();
            // Use shared function to update all dashboard values
            updateDashboardFromValuation(valuation);
        } else {
            // If valuation fails, just show cash
            document.getElementById('total-value').textContent = `$${summary.cash.toFixed(2)}`;
            document.getElementById('position-value').textContent = '$0.00';
            document.getElementById('total-pnl').textContent = '$0.00';
            document.getElementById('pnl-percentage').textContent = '0.00%';
            document.getElementById('stock-pnl').textContent = '$0.00';
            document.getElementById('stock-pnl-percentage').textContent = '0.00%';
            document.getElementById('stock-value').textContent = '$0.00';
            document.getElementById('option-pnl').textContent = '$0.00';
            document.getElementById('option-pnl-percentage').textContent = '0.00%';
            document.getElementById('option-value').textContent = '$0.00';
        }

        // Hide loading modal
        hideLoadingModal();

    } catch (error) {
        console.error('Error loading portfolio valuation:', error);
        showError('Error loading portfolio data');
        // Hide loading modal on error too
        hideLoadingModal();
    }
}

// Load portfolio valuation with user-specified market parameters (from modal)
async function loadPortfolioValuationWithParams(portfolioId, marketParams) {
    try {
        showLoadingModal();

        const summaryResponse = await fetch(`${API_BASE}/api/portfolio/${portfolioId}/summary`);
        const summary = await summaryResponse.json();
        document.getElementById('cash-balance').textContent = `$${summary.cash.toFixed(2)}`;

        const valuationResponse = await fetch(`${API_BASE}/api/portfolio/${portfolioId}/value`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(marketParams)
        });

        if (valuationResponse.ok) {
            const valuation = await valuationResponse.json();
            updateDashboardFromValuation(valuation);
        } else {
            document.getElementById('total-value').textContent = `$${summary.cash.toFixed(2)}`;
            document.getElementById('position-value').textContent = '$0.00';
            document.getElementById('total-pnl').textContent = '$0.00';
            document.getElementById('pnl-percentage').textContent = '0.00%';
            document.getElementById('stock-pnl').textContent = '$0.00';
            document.getElementById('stock-pnl-percentage').textContent = '0.00%';
            document.getElementById('stock-value').textContent = '$0.00';
            document.getElementById('option-pnl').textContent = '$0.00';
            document.getElementById('option-pnl-percentage').textContent = '0.00%';
            document.getElementById('option-value').textContent = '$0.00';
        }

        hideLoadingModal();
    } catch (error) {
        console.error('Error loading portfolio valuation:', error);
        showError('Error loading portfolio data');
        hideLoadingModal();
    }
}

// ==================== Options View ====================

async function loadOptionsTable() {
    const tableContainer = document.getElementById('options-table');
    tableContainer.innerHTML = '<div class="loading">Loading options</div>';

    try {
        const response = await fetch(`${API_BASE}/api/options`);
        const data = await response.json();
        const options = data.options || [];

        if (options.length === 0) {
            tableContainer.innerHTML = '<p>No options found. Create one above!</p>';
            return;
        }

        const optionTypeNames = ['European', 'Asian', 'Digital', 'Barrier', 'Lookback', 'Range'];

        const table = `
            <table>
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Stock</th>
                        <th>Exchange</th>
                        <th>Type</th>
                        <th>Call/Put</th>
                        <th>Strike</th>
                        <th>Expiry</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    ${options.map(opt => `
                        <tr>
                            <td>${opt.id}</td>
                            <td>${opt.stock ? opt.stock.ticker : 'N/A'}</td>
                            <td>${opt.stock && opt.stock.exchangeName ? opt.stock.exchangeName : 'N/A'}</td>
                            <td>${optionTypeNames[opt.optionParameters.optionType]}</td>
                            <td>${opt.optionParameters.isCall ? 'Call' : 'Put'}</td>
                            <td>${opt.optionParameters.strike}</td>
                            <td>${new Date(opt.optionParameters.expiryDate).toLocaleDateString()}</td>
                            <td><button class="btn-delete" onclick="deleteOption(${opt.id})">Delete</button></td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        `;

        tableContainer.innerHTML = table;

    } catch (error) {
        tableContainer.innerHTML = `<div class="error-box">Error loading options: ${error.message}</div>`;
    }
}

async function deleteOption(optionId) {
    if (!confirm(`Are you sure you want to delete option #${optionId}?`)) return;

    try {
        const response = await fetch(`${API_BASE}/api/options/${optionId}`, {
            method: 'DELETE'
        });

        if (response.ok) {
            showSuccess(`Option #${optionId} deleted successfully`);
            await loadOptionsTable();
            await loadOptions(); // Refresh options list for dropdowns
        } else {
            // Try to parse JSON error, but fall back to text if it fails
            let errorMessage = 'Failed to delete option';
            try {
                const error = await response.json();
                errorMessage = error.message || errorMessage;
            } catch (e) {
                // If response is not JSON (e.g., HTML error page), use status text
                errorMessage = `Failed to delete option: ${response.statusText}. The option may be referenced by positions or trades.`;
            }
            showError(errorMessage);
        }
    } catch (error) {
        showError(`Error deleting option: ${error.message}`);
    }
}

// Load full stocks universe table (for the View Stocks page)
// ==================== Exchange Management ====================

async function loadExchangesTable() {
    const tableContainer = document.getElementById('exchanges-table');
    if (!tableContainer) return;

    tableContainer.innerHTML = '<div class="loading">Loading exchanges...</div>';

    try {
        const response = await fetch(`${API_BASE}/api/exchange`);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        exchanges = await response.json();

        if (!exchanges || exchanges.length === 0) {
            tableContainer.innerHTML = '<p>No exchanges found. Create one using the form above!</p>';
            return;
        }

        const table = `
            <table>
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Description</th>
                        <th>Country</th>
                        <th>Currency</th>
                        <th>Created</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    ${exchanges.map(exchange => `
                        <tr>
                            <td><strong>${exchange.name}</strong></td>
                            <td>${exchange.description || 'N/A'}</td>
                            <td>${exchange.country || 'N/A'}</td>
                            <td>${exchange.currency || 'N/A'}</td>
                            <td>${new Date(exchange.createdAt).toLocaleDateString()}</td>
                            <td>
                                <button class="btn btn-danger btn-sm" onclick="deleteExchange(${exchange.id}, '${exchange.name}')">Delete</button>
                            </td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        `;

        tableContainer.innerHTML = table;

    } catch (error) {
        tableContainer.innerHTML = `<div class="error-box">Error loading exchanges: ${error.message}</div>`;
    }
}

async function handleCreateExchange(e) {
    e.preventDefault();

    const name = document.getElementById('exchange-name').value.trim();
    const description = document.getElementById('exchange-description').value.trim();
    const country = document.getElementById('exchange-country').value.trim();
    const currency = document.getElementById('exchange-currency').value.trim().toUpperCase();

    if (!name) {
        showError('Exchange name is required');
        return;
    }

    // Warn user about non-USD currency rate curve limitation
    if (currency && currency !== 'USD') {
        const proceed = confirm(
            `⚠️ Currency Notice: ${currency}\n\n` +
            `This system uses a USD-based risk-free rate curve for all option pricing calculations.\n\n` +
            `For exchanges with non-USD currencies (like ${currency}), the pricing will still use USD Treasury rates ` +
            `rather than the appropriate local rates (e.g., UK Gilts for GBP, JGBs for JPY).\n\n` +
            `This is a simplification for this implementation.\n\n` +
            `Do you want to continue creating this exchange?`
        );
        if (!proceed) {
            return;
        }
    }

    try {
        const response = await fetch(`${API_BASE}/api/exchange`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                name,
                description: description || null,
                country: country || null,
                currency: currency || null
            })
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Failed to create exchange');
        }

        // Reset form
        e.target.reset();

        // Reload exchanges table
        await loadExchangesTable();

        showSuccess('Exchange created successfully!');

    } catch (error) {
        showError('Error creating exchange: ' + error.message);
    }
}

async function deleteExchange(id, name) {
    if (!confirm(`Are you sure you want to delete exchange "${name}"?`)) {
        return;
    }

    try {
        const response = await fetch(`${API_BASE}/api/exchange/${id}`, {
            method: 'DELETE'
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Failed to delete exchange');
        }

        // Reload exchanges table
        await loadExchangesTable();

        showSuccess('Exchange deleted successfully!');

    } catch (error) {
        showError('Error deleting exchange: ' + error.message);
    }
}

// ==================== Stocks View ====================

async function loadStocksUniverseTable() {
    const tableContainer = document.getElementById('stocks-universe-table');
    if (!tableContainer) return;

    tableContainer.innerHTML = '<div class="loading">Loading stocks...</div>';

    try {
        const response = await fetch(`${API_BASE}/api/stock`);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const stocks = await response.json();

        if (!stocks || stocks.length === 0) {
            tableContainer.innerHTML = '<p>No stocks found. Add one using the button above!</p>';
            return;
        }

        const table = `
            <table>
                <thead>
                    <tr>
                        <th>Ticker</th>
                        <th>Name</th>
                        <th>Exchange</th>
                        <th>Current Price</th>
                        <th>Description</th>
                    </tr>
                </thead>
                <tbody>
                    ${stocks.map(stock => `
                        <tr>
                            <td><strong>${stock.ticker}</strong></td>
                            <td>${stock.name}</td>
                            <td>${stock.exchangeName || 'N/A'}</td>
                            <td>$${stock.currentPrice.toFixed(2)}</td>
                            <td>${stock.description || 'N/A'}</td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        `;

        tableContainer.innerHTML = table;

    } catch (error) {
        tableContainer.innerHTML = `<div class="error-box">Error loading stocks: ${error.message}</div>`;
    }
}

// ==================== Trades View ====================

async function loadTradesView() {
    // Populate price-trade-option selector
    await updateTradeOptionSelectors();

    // Populate trade-stock selector
    updateStockSelectors();

    const portfolioId = document.getElementById('trade-portfolio').value;
    if (portfolioId) {
        await loadTradesTable(parseInt(portfolioId));
    }

    // Add listener to reload trades when portfolio changes
    const portfolioSelector = document.getElementById('trade-portfolio');
    portfolioSelector.removeEventListener('change', handleTradePortfolioChange);
    portfolioSelector.addEventListener('change', handleTradePortfolioChange);

    // Add listener for asset type changes
    const assetTypeSelector = document.getElementById('trade-asset-type');
    assetTypeSelector.removeEventListener('change', handleAssetTypeChange);
    assetTypeSelector.addEventListener('change', handleAssetTypeChange);

    // Initialize asset type display
    handleAssetTypeChange();
}

function handleAssetTypeChange() {
    const assetType = document.getElementById('trade-asset-type').value;
    const stockGroup = document.getElementById('stock-selector-group');
    const optionGroup = document.getElementById('option-selector-group');
    const pricingSection = document.getElementById('option-pricing-section');
    const priceLabel = document.getElementById('price-label');

    if (assetType === '0') {
        // Stock selected
        stockGroup.style.display = 'block';
        optionGroup.style.display = 'none';
        pricingSection.style.display = 'none';
        priceLabel.textContent = 'Price per Share:';

        // Clear option selection
        document.getElementById('trade-option').value = '';
    } else {
        // Option selected
        stockGroup.style.display = 'none';
        optionGroup.style.display = 'block';
        pricingSection.style.display = 'block';
        priceLabel.textContent = 'Price per Contract:';

        // Clear stock selection
        document.getElementById('trade-stock').value = '';
    }
}

async function updateTradeOptionSelectors() {
    const priceSelector = document.getElementById('price-trade-option');
    priceSelector.innerHTML = '<option value="">-- Select Option --</option>';

    options.forEach(option => {
        const optionElement = document.createElement('option');
        const optionTypeNames = ['European', 'Asian', 'Digital', 'Barrier', 'Lookback', 'Range'];
        const typeName = optionTypeNames[option.optionParameters.optionType];
        const callPut = option.optionParameters.isCall ? 'Call' : 'Put';
        const ticker = option.stock?.ticker || 'N/A';

        optionElement.value = option.id;
        optionElement.textContent = `#${option.id} - ${typeName} ${callPut} @ ${option.optionParameters.strike} [${ticker}]`;
        priceSelector.appendChild(optionElement);
    });
}

async function handleTradePortfolioChange(e) {
    const portfolioId = e.target.value;
    if (portfolioId) {
        await loadTradesTable(parseInt(portfolioId));
    }
}

async function loadTradesTable(portfolioId) {
    const tableContainer = document.getElementById('trades-table');
    tableContainer.innerHTML = '<div class="loading">Loading trades</div>';

    try {
        // Fetch trades, options, and stocks in parallel
        const [tradesResponse, optionsResponse, stocksResponse] = await Promise.all([
            fetch(`${API_BASE}/api/portfolio/${portfolioId}/trades`),
            fetch(`${API_BASE}/api/options`),
            fetch(`${API_BASE}/api/stock`)
        ]);

        const trades = await tradesResponse.json();
        const optionsData = await optionsResponse.json();
        const stocksData = await stocksResponse.json();

        // Create lookup maps
        const optionsMap = {};
        const stocksMap = {};

        if (optionsData.options) {
            optionsData.options.forEach(opt => {
                const optionTypeNames = ['European', 'Asian', 'Digital', 'Barrier', 'Lookback', 'Range'];
                optionsMap[opt.id] = {
                    type: optionTypeNames[opt.optionParameters.optionType],
                    strike: opt.optionParameters.strike,
                    isCall: opt.optionParameters.isCall,
                    ticker: opt.stock?.ticker || 'N/A',
                    expiryDate: opt.optionParameters.expiryDate
                };
            });
        }

        stocksData.forEach(stock => {
            stocksMap[stock.id] = {
                ticker: stock.ticker,
                name: stock.name
            };
        });

        if (trades.length === 0) {
            tableContainer.innerHTML = '<p>No trades found for this portfolio.</p>';
            return;
        }

        const tradeTypeNames = ['Buy', 'Sell', 'Close'];
        const assetTypeNames = ['Stock', 'Option'];

        const table = `
            <p class="table-hint">Click on an option row to price it</p>
            <table>
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Asset Type</th>
                        <th>Asset</th>
                        <th>Strike/Price</th>
                        <th>Expiry</th>
                        <th>Trade</th>
                        <th>Qty</th>
                        <th>Price</th>
                        <th>Total Cost</th>
                        <th>Date</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    ${trades.map(trade => {
                        const isStock = trade.assetType === 0;
                        let assetDisplay, strikeDisplay, expiryDisplay;

                        if (isStock) {
                            const stockInfo = stocksMap[trade.stockId] || { ticker: 'Unknown', name: '' };
                            assetDisplay = `${stockInfo.ticker}`;
                            strikeDisplay = '--';
                            expiryDisplay = '--';
                        } else {
                            const optionInfo = optionsMap[trade.optionId] || { type: 'Unknown', strike: 'N/A', isCall: true, ticker: 'N/A', expiryDate: null };
                            const callPut = optionInfo.isCall ? 'Call' : 'Put';
                            assetDisplay = `#${trade.optionId} ${optionInfo.type} ${callPut} [${optionInfo.ticker}]`;
                            strikeDisplay = typeof optionInfo.strike === 'number' ? optionInfo.strike.toFixed(2) : optionInfo.strike;
                            expiryDisplay = optionInfo.expiryDate ? new Date(optionInfo.expiryDate).toLocaleDateString() : 'N/A';
                        }

                        const rowClass = isStock ? 'trade-row stock-row' : 'trade-row option-row clickable';
                        const rowClick = isStock ? '' : `onclick="priceOptionFromTrade(${trade.optionId})"`;

                        return `
                        <tr class="${rowClass}" ${rowClick}>
                            <td>${trade.id}</td>
                            <td><span class="asset-type-badge ${isStock ? 'stock' : 'option'}">${assetTypeNames[trade.assetType]}</span></td>
                            <td>${assetDisplay}</td>
                            <td>${strikeDisplay}</td>
                            <td>${expiryDisplay}</td>
                            <td>${tradeTypeNames[trade.tradeType]}</td>
                            <td>${trade.quantity}</td>
                            <td>$${trade.price.toFixed(2)}</td>
                            <td>$${trade.totalCost.toFixed(2)}</td>
                            <td>${new Date(trade.tradeDate).toLocaleDateString()}</td>
                            <td><button class="btn-delete" onclick="event.stopPropagation(); deleteTrade(${portfolioId}, ${trade.id})">Delete</button></td>
                        </tr>
                        `;
                    }).join('')}
                </tbody>
            </table>
        `;

        tableContainer.innerHTML = table;

    } catch (error) {
        tableContainer.innerHTML = `<div class="error-box">Error loading trades: ${error.message}</div>`;
    }
}

// Price an option when clicking on a trade row
async function priceOptionFromTrade(optionId) {
    // Fetch the option to get its details for the modal
    try {
        const response = await fetch(`${API_BASE}/api/options/${optionId}`);
        if (!response.ok) {
            showError('Could not load option details');
            return;
        }
        const option = await response.json();

        // Populate modal fields with option info
        document.getElementById('param-stock-price').value = option.stock?.currentPrice || 100;

        // Calculate time to expiry
        if (option.optionParameters.expiryDate) {
            const expiry = new Date(option.optionParameters.expiryDate);
            const now = new Date();
            const years = (expiry - now) / (365.25 * 24 * 60 * 60 * 1000);
            document.getElementById('param-time-to-expiry').value = years.toFixed(4);
        }

        // Ensure modal is initialized
        initializeMarketParamsModal();

        // Set pending action to price this specific option
        pendingPricingAction = async function(marketParams) {
            await priceSpecificOption(optionId, marketParams);
        };

        // Show the modal
        document.getElementById('market-params-modal').classList.add('show');

    } catch (error) {
        console.error('Error loading option for pricing:', error);
        showError('Error loading option details');
    }
}

// Price a specific option and show results in a popup modal
async function priceSpecificOption(optionId, marketParams) {
    try {
        showLoadingModal();

        // Correct API endpoint: /api/pricing/{optionId} (no /price/ in path)
        const response = await fetch(`${API_BASE}/api/pricing/${optionId}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(marketParams)
        });

        if (!response.ok) {
            hideLoadingModal();
            const errorData = await response.json().catch(() => ({ message: 'Unknown error' }));
            showError('Error pricing option: ' + (errorData.message || 'Request failed'));
            return;
        }

        const data = await response.json();
        hideLoadingModal();

        // Extract the pricing result from the response
        const result = data.pricingResult;
        const option = data.option;

        // Build option description
        const optionTypeNames = ['European', 'Asian', 'Digital', 'Barrier', 'Lookback', 'Range'];
        const typeName = option?.optionParameters ? optionTypeNames[option.optionParameters.optionType] : 'Option';
        const callPut = option?.optionParameters?.isCall ? 'Call' : 'Put';
        const ticker = option?.stock?.ticker || 'N/A';
        const strike = option?.optionParameters?.strike?.toFixed(2) || 'N/A';

        // Show pricing result in the popup modal
        const resultContent = document.getElementById('option-pricing-result-content');
        resultContent.innerHTML = `
            <div class="pricing-result-popup">
                <h4>${typeName} ${callPut} @ ${strike} [${ticker}]</h4>
                <div class="price-display">$${result.price.toFixed(4)}</div>

                <div class="pricing-section">
                    <h5>Market Parameters</h5>
                    <div class="detail-grid">
                        <div class="detail-item">
                            <div class="detail-label">Stock Price</div>
                            <div class="detail-value">$${data.simulationParameters?.initialPrice?.toFixed(2) || 'N/A'}</div>
                        </div>
                        <div class="detail-item">
                            <div class="detail-label">Time to Expiry</div>
                            <div class="detail-value">${data.simulationParameters?.timeToExpiry?.toFixed(4) || 'N/A'} yrs</div>
                        </div>
                        <div class="detail-item">
                            <div class="detail-label">Risk-Free Rate</div>
                            <div class="detail-value">${data.simulationParameters?.riskFreeRate ? (data.simulationParameters.riskFreeRate * 100).toFixed(2) + '%' : 'N/A'}</div>
                        </div>
                        <div class="detail-item">
                            <div class="detail-label">Standard Error</div>
                            <div class="detail-value">${result.standardError?.toFixed(6) || 'N/A'}</div>
                        </div>
                    </div>
                </div>

                <div class="pricing-section">
                    <h5>Greeks</h5>
                    <div class="greeks-grid">
                        <div class="greek-item">
                            <div class="greek-label">Delta (Δ)</div>
                            <div class="greek-value">${result.delta?.toFixed(4) || 'N/A'}</div>
                        </div>
                        <div class="greek-item">
                            <div class="greek-label">Gamma (Γ)</div>
                            <div class="greek-value">${result.gamma?.toFixed(4) || 'N/A'}</div>
                        </div>
                        <div class="greek-item">
                            <div class="greek-label">Vega (ν)</div>
                            <div class="greek-value">${result.vega?.toFixed(4) || 'N/A'}</div>
                        </div>
                        <div class="greek-item">
                            <div class="greek-label">Theta (Θ)</div>
                            <div class="greek-value">${result.theta?.toFixed(4) || 'N/A'}</div>
                        </div>
                        <div class="greek-item">
                            <div class="greek-label">Rho (ρ)</div>
                            <div class="greek-value">${result.rho?.toFixed(4) || 'N/A'}</div>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Show the pricing result modal
        const modal = document.getElementById('option-pricing-result-modal');
        modal.classList.add('show');

        // Initialize close button handler (only once)
        initializePricingResultModal();

    } catch (error) {
        hideLoadingModal();
        console.error('Error pricing option:', error);
        showError('Error pricing option: ' + error.message);
    }
}

// Track if pricing result modal has been initialized
let pricingResultModalInitialized = false;

// Initialize pricing result modal event listeners
function initializePricingResultModal() {
    if (pricingResultModalInitialized) return;
    pricingResultModalInitialized = true;

    const modal = document.getElementById('option-pricing-result-modal');
    const closeBtn = document.getElementById('close-pricing-result-modal');

    closeBtn.addEventListener('click', () => {
        modal.classList.remove('show');
    });

    // Close on outside click
    modal.addEventListener('click', (e) => {
        if (e.target === modal) {
            modal.classList.remove('show');
        }
    });
}

async function deleteTrade(portfolioId, tradeId) {
    if (!confirm(`Are you sure you want to delete trade #${tradeId}?`)) return;

    try {
        const response = await fetch(`${API_BASE}/api/portfolio/${portfolioId}/trades/${tradeId}`, {
            method: 'DELETE'
        });

        if (response.ok) {
            showSuccess(`Trade #${tradeId} deleted successfully`);
            await loadTradesTable(portfolioId);
        } else {
            const error = await response.json();
            showError(error.message || 'Failed to delete trade');
        }
    } catch (error) {
        showError(`Error deleting trade: ${error.message}`);
    }
}

// ==================== Positions View ====================

async function loadPositionsView() {
    const portfolioId = document.getElementById('positions-portfolio').value;
    if (portfolioId) {
        await loadPositions(parseInt(portfolioId));
    }
}

async function loadPositions(portfolioId) {
    const tableContainer = document.getElementById('positions-table');
    tableContainer.innerHTML = '<div class="loading">Loading positions</div>';

    try {
        const response = await fetch(`${API_BASE}/api/portfolio/${portfolioId}/positions`);
        const positions = await response.json();

        if (positions.length === 0) {
            tableContainer.innerHTML = '<p>No open positions in this portfolio.</p>';
            return;
        }

        const optionTypeNames = ['European', 'Asian', 'Digital', 'Barrier', 'Lookback', 'Range'];
        const assetTypeNames = ['Stock', 'Option'];

        const table = `
            <table>
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Asset Type</th>
                        <th>Asset</th>
                        <th>Expiry</th>
                        <th>Net Qty</th>
                        <th>Avg Cost</th>
                        <th>Total Cost</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    ${positions.map(pos => {
                        const isStock = pos.assetType === 0;
                        let assetDisplay, expiryDisplay;

                        if (isStock) {
                            // Stock position
                            assetDisplay = pos.stock ? `${pos.stock.ticker} - ${pos.stock.name}` : `Stock #${pos.stockId}`;
                            expiryDisplay = '--';
                        } else {
                            // Option position
                            if (pos.option) {
                                const typeName = optionTypeNames[pos.option.optionParameters.optionType];
                                const callPut = pos.option.optionParameters.isCall ? 'Call' : 'Put';
                                const ticker = pos.option.stock?.ticker || 'N/A';
                                const strike = pos.option.optionParameters.strike;
                                assetDisplay = `${typeName} ${callPut} @ ${strike.toFixed(2)} [${ticker}]`;
                                expiryDisplay = pos.option.optionParameters.expiryDate
                                    ? new Date(pos.option.optionParameters.expiryDate).toLocaleDateString()
                                    : 'N/A';
                            } else {
                                assetDisplay = `Option #${pos.optionId}`;
                                expiryDisplay = 'N/A';
                            }
                        }

                        return `
                        <tr>
                            <td>${pos.id}</td>
                            <td><span class="asset-type-badge ${isStock ? 'stock' : 'option'}">${assetTypeNames[pos.assetType]}</span></td>
                            <td>${assetDisplay}</td>
                            <td>${expiryDisplay}</td>
                            <td>${pos.netQuantity}</td>
                            <td>$${pos.averageCost.toFixed(2)}</td>
                            <td>$${pos.totalCost.toFixed(2)}</td>
                            <td><button class="btn-delete" onclick="deletePosition(${portfolioId}, ${pos.id})">Delete</button></td>
                        </tr>
                        `;
                    }).join('')}
                </tbody>
            </table>
        `;

        tableContainer.innerHTML = table;

    } catch (error) {
        tableContainer.innerHTML = `<div class="error-box">Error loading positions: ${error.message}</div>`;
    }
}

async function deletePosition(portfolioId, positionId) {
    if (!confirm(`Are you sure you want to delete position #${positionId}?`)) return;

    try {
        const response = await fetch(`${API_BASE}/api/portfolio/${portfolioId}/positions/${positionId}`, {
            method: 'DELETE'
        });

        if (response.ok) {
            showSuccess(`Position #${positionId} deleted successfully`);
            await loadPositions(portfolioId);
        } else {
            const error = await response.json();
            showError(error.message || 'Failed to delete position');
        }
    } catch (error) {
        showError(`Error deleting position: ${error.message}`);
    }
}

// ==================== Portfolio View ====================

async function loadPortfoliosTable() {
    const tableContainer = document.getElementById('portfolios-table');
    tableContainer.innerHTML = '<div class="loading">Loading portfolios</div>';

    try {
        const response = await fetch(`${API_BASE}/api/portfolio`);
        const portfolios = await response.json();

        // Update portfolio detail selector
        const detailSelector = document.getElementById('portfolio-detail-select');
        detailSelector.innerHTML = '<option value="">-- Select Portfolio --</option>';
        portfolios.forEach(portfolio => {
            const option = document.createElement('option');
            option.value = portfolio.id;
            option.textContent = `${portfolio.name} ($${portfolio.cash.toFixed(2)})`;
            detailSelector.appendChild(option);
        });

        if (portfolios.length === 0) {
            tableContainer.innerHTML = '<p>No portfolios found. Create one from the Home view!</p>';
            return;
        }

        const table = `
            <table>
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Name</th>
                        <th>Description</th>
                        <th>Cash</th>
                        <th>Created</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    ${portfolios.map(portfolio => `
                        <tr>
                            <td>${portfolio.id}</td>
                            <td><strong>${portfolio.name}</strong></td>
                            <td>${portfolio.description || 'N/A'}</td>
                            <td>$${portfolio.cash.toFixed(2)}</td>
                            <td>${new Date(portfolio.createdAt).toLocaleDateString()}</td>
                            <td><button class="btn-delete" onclick="deletePortfolio(${portfolio.id})">Delete</button></td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        `;

        tableContainer.innerHTML = table;

    } catch (error) {
        tableContainer.innerHTML = `<div class="error-box">Error loading portfolios: ${error.message}</div>`;
    }
}

async function deletePortfolio(portfolioId) {
    if (!confirm(`Are you sure you want to delete portfolio #${portfolioId}? This will delete all positions and trades in the portfolio.`)) return;

    try {
        const response = await fetch(`${API_BASE}/api/portfolio/${portfolioId}`, {
            method: 'DELETE'
        });

        if (response.ok) {
            showSuccess(`Portfolio #${portfolioId} deleted successfully`);
            // Reload both the select dropdown and the portfolios table
            await loadPortfolios();
            await loadPortfoliosTable();
        } else {
            const error = await response.json();
            showError(error.message || 'Failed to delete portfolio');
        }
    } catch (error) {
        showError(`Error deleting portfolio: ${error.message}`);
    }
}

// ==================== Utility Functions ====================

function showSuccess(message) {
    // Simple alert for now - could be enhanced with a toast notification
    const successBox = document.createElement('div');
    successBox.className = 'success-box';
    successBox.textContent = message;
    document.querySelector('.main-content').prepend(successBox);

    setTimeout(() => successBox.remove(), 3000);
}

function showError(message) {
    const errorBox = document.createElement('div');
    errorBox.className = 'error-box';
    errorBox.textContent = message;
    document.querySelector('.main-content').prepend(errorBox);

    setTimeout(() => errorBox.remove(), 5000);
}

// ==================== Pricing Functions ====================

// Store which pricing action to execute after parameters are set
let pendingPricingAction = null;

// Default market parameters for pricing
// Note: riskFreeRate and timeToExpiry are auto-calculated per option
const defaultMarketParams = {
    volatility: 0.2,
    timeSteps: 252,
    numberOfPaths: 10000,
    useMultithreading: true,
    simMode: 0
};

// Rate curve for client-side display (matches server-side appsettings.json)
const rateCurve = [
    { tenorYears: 0.0833, rate: 0.0525 },
    { tenorYears: 0.25, rate: 0.0510 },
    { tenorYears: 0.5, rate: 0.0485 },
    { tenorYears: 1.0, rate: 0.0455 },
    { tenorYears: 2.0, rate: 0.0430 },
    { tenorYears: 3.0, rate: 0.0420 },
    { tenorYears: 5.0, rate: 0.0415 }
];

// Calculate time to expiry in years
function calculateTimeToExpiry(expiryDate) {
    const now = new Date();
    const expiry = new Date(expiryDate);
    const diffMs = expiry - now;
    const diffYears = diffMs / (365.25 * 24 * 60 * 60 * 1000);
    return Math.max(0, diffYears);
}

// Linear interpolation of risk-free rate from curve
function getRateFromCurve(timeToExpiry) {
    if (timeToExpiry <= 0) return rateCurve[0].rate;

    // Find surrounding points
    for (let i = 0; i < rateCurve.length - 1; i++) {
        if (timeToExpiry >= rateCurve[i].tenorYears && timeToExpiry <= rateCurve[i + 1].tenorYears) {
            // Linear interpolation
            const t1 = rateCurve[i].tenorYears;
            const t2 = rateCurve[i + 1].tenorYears;
            const r1 = rateCurve[i].rate;
            const r2 = rateCurve[i + 1].rate;
            return r1 + (r2 - r1) * (timeToExpiry - t1) / (t2 - t1);
        }
    }

    // Flat extrapolation at edges
    if (timeToExpiry < rateCurve[0].tenorYears) {
        return rateCurve[0].rate;
    }
    return rateCurve[rateCurve.length - 1].rate;
}

// Show market parameters modal
async function showMarketParamsModal(pricingAction, optionId) {
    pendingPricingAction = pricingAction;

    // Reset rate/expiry fields to defaults
    document.getElementById('param-time-to-expiry').value = '--';
    document.getElementById('param-risk-free-rate').value = '--';

    // If optionId is provided, fetch the option and auto-populate fields
    if (optionId) {
        const option = options.find(o => o.id === parseInt(optionId));
        if (option) {
            // Populate stock price
            if (option.stock) {
                document.getElementById('param-stock-price').value = option.stock.currentPrice.toFixed(2);
            }

            // Calculate and display time to expiry
            if (option.optionParameters && option.optionParameters.expiryDate) {
                const timeToExpiry = calculateTimeToExpiry(option.optionParameters.expiryDate);
                document.getElementById('param-time-to-expiry').value = timeToExpiry.toFixed(4) + ' yrs';

                // Calculate and display risk-free rate
                const riskFreeRate = getRateFromCurve(timeToExpiry);
                document.getElementById('param-risk-free-rate').value = (riskFreeRate * 100).toFixed(2) + '%';
            }
        }
    } else {
        // For portfolio pricing (multiple options), show indication
        document.getElementById('param-stock-price').value = 'Varies';
        document.getElementById('param-time-to-expiry').value = 'Per option';
        document.getElementById('param-risk-free-rate').value = 'Per option';
    }

    const modal = document.getElementById('market-params-modal');
    modal.classList.add('show');
}

// Close market parameters modal
function closeMarketParamsModal() {
    const modal = document.getElementById('market-params-modal');
    modal.classList.remove('show');
    pendingPricingAction = null;
}

// Track if modal has been initialized
let marketParamsModalInitialized = false;

// Initialize market parameters modal event listeners
function initializeMarketParamsModal() {
    // Prevent duplicate initialization
    if (marketParamsModalInitialized) return;
    marketParamsModalInitialized = true;

    const paramsForm = document.getElementById('market-params-form');
    const closeBtn = document.getElementById('close-params-modal');
    const modal = document.getElementById('market-params-modal');

    paramsForm.addEventListener('submit', function(e) {
        e.preventDefault();

        // Get user-entered parameters
        // Note: riskFreeRate and timeToExpiry are now auto-calculated from rate curve and option expiry
        const marketParams = {
            volatility: parseFloat(document.getElementById('param-volatility').value),
            timeSteps: parseInt(document.getElementById('param-time-steps').value),
            numberOfPaths: parseInt(document.getElementById('param-paths').value),
            useMultithreading: true,
            simMode: parseInt(document.getElementById('param-sim-mode').value)
        };

        console.log('Market params submitted:', marketParams);

        // Save the action before closing (closeMarketParamsModal sets it to null)
        const actionToExecute = pendingPricingAction;

        // Close modal
        closeMarketParamsModal();

        // Execute the saved action
        if (actionToExecute) {
            console.log('Executing pending pricing action');
            actionToExecute(marketParams);
        } else {
            console.log('No pending pricing action!');
        }
    });

    closeBtn.addEventListener('click', closeMarketParamsModal);

    // Close modal when clicking outside
    modal.addEventListener('click', function(e) {
        if (e.target === modal) {
            closeMarketParamsModal();
        }
    });
}

async function priceTradeOption() {
    const optionId = document.getElementById('price-trade-option').value;
    if (!optionId) {
        showError('Please select an option to price');
        return;
    }

    // Show modal and pass the actual pricing function (with optionId for auto-population)
    showMarketParamsModal(async (marketParams) => {
        console.log('Pricing trade option with params:', marketParams);
        const resultContainer = document.getElementById('trade-pricing-result');
        resultContainer.innerHTML = '<div class="loading">Pricing option</div>';
        resultContainer.style.display = 'block';

        try {
            console.log(`Fetching: ${API_BASE}/api/pricing/${optionId}`);
            const response = await fetch(`${API_BASE}/api/pricing/${optionId}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(marketParams)
            });

            console.log('Response status:', response.status);

            if (!response.ok) {
                const errorData = await response.json();
                console.error('Error response:', errorData);
                throw new Error(errorData.message || 'Failed to price option');
            }

            const data = await response.json();
            console.log('Pricing data received:', data);
            displayPricingResult(data, resultContainer);

            // Auto-populate the price field
            document.getElementById('price').value = data.pricingResult.price.toFixed(2);

        } catch (error) {
            console.error('Pricing error:', error);
            resultContainer.innerHTML = `<div class="error-box">Error pricing option: ${error.message}</div>`;
        }
    }, optionId);
}

async function priceAllPositions() {
    const portfolioId = document.getElementById('positions-portfolio').value;
    if (!portfolioId) {
        showError('Please select a portfolio first');
        return;
    }

    // Show modal and pass the actual pricing function
    showMarketParamsModal(async (marketParams) => {
        const resultContainer = document.getElementById('positions-valuation');
        resultContainer.innerHTML = '<div class="loading">Pricing all positions</div>';
        resultContainer.style.display = 'block';

        try {
            const response = await fetch(`${API_BASE}/api/portfolio/${portfolioId}/value`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(marketParams)
            });

            if (!response.ok) throw new Error('Failed to value portfolio');

            const valuation = await response.json();
            displayPortfolioValuation(valuation, resultContainer);

            // Update home dashboard if this is the current portfolio
            if (currentPortfolioId == portfolioId) {
                updateDashboardFromValuation(valuation);
            }

        } catch (error) {
            resultContainer.innerHTML = `<div class="error-box">Error valuing positions: ${error.message}</div>`;
        }
    });
}

async function priceSelectedPortfolio() {
    const portfolioId = document.getElementById('portfolio-detail-select').value;
    if (!portfolioId) {
        showError('Please select a portfolio first');
        return;
    }

    // Show modal and pass the actual pricing function
    showMarketParamsModal(async (marketParams) => {
        const resultContainer = document.getElementById('portfolio-detail-summary');
        resultContainer.innerHTML = '<div class="loading">Pricing portfolio</div>';
        resultContainer.style.display = 'block';

        try {
            const response = await fetch(`${API_BASE}/api/portfolio/${portfolioId}/value`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(marketParams)
            });

            if (!response.ok) throw new Error('Failed to value portfolio');

            const valuation = await response.json();
            displayPortfolioValuation(valuation, resultContainer);

            // Update home dashboard if this is the current portfolio
            if (currentPortfolioId == portfolioId) {
                updateDashboardFromValuation(valuation);
            }

        } catch (error) {
            resultContainer.innerHTML = `<div class="error-box">Error valuing portfolio: ${error.message}</div>`;
        }
    });
}

function displayPricingResult(data, container) {
    const result = data.pricingResult;

    const html = `
        <h3>Option Pricing Result</h3>
        <div class="price-display">$${result.price.toFixed(4)}</div>
        <div class="detail-grid">
            <div class="detail-item">
                <div class="detail-label">Standard Error</div>
                <div class="detail-value">${result.standardError?.toFixed(6) || 'N/A'}</div>
            </div>
            <div class="detail-item">
                <div class="detail-label">Delta (Δ)</div>
                <div class="detail-value">${result.delta?.toFixed(4) || 'N/A'}</div>
            </div>
            <div class="detail-item">
                <div class="detail-label">Gamma (Γ)</div>
                <div class="detail-value">${result.gamma?.toFixed(4) || 'N/A'}</div>
            </div>
            <div class="detail-item">
                <div class="detail-label">Vega (ν)</div>
                <div class="detail-value">${result.vega?.toFixed(4) || 'N/A'}</div>
            </div>
            <div class="detail-item">
                <div class="detail-label">Theta (Θ)</div>
                <div class="detail-value">${result.theta?.toFixed(4) || 'N/A'}</div>
            </div>
            <div class="detail-item">
                <div class="detail-label">Rho (ρ)</div>
                <div class="detail-value">${result.rho?.toFixed(4) || 'N/A'}</div>
            </div>
        </div>
    `;

    container.innerHTML = html;
}

function displayPortfolioValuation(valuation, container) {
    // Separate stock and option positions
    const stockPositions = valuation.positions.filter(p => p.assetType === 0);
    const optionPositions = valuation.positions.filter(p => p.assetType === 1);

    // Calculate separate P&L for stocks and options
    const stockPnL = stockPositions.reduce((sum, p) => sum + p.unrealizedPnL, 0);
    const optionPnL = optionPositions.reduce((sum, p) => sum + p.unrealizedPnL, 0);
    const stockCost = stockPositions.reduce((sum, p) => sum + p.totalCost, 0);
    const optionCost = optionPositions.reduce((sum, p) => sum + p.totalCost, 0);
    const stockPnLPct = stockCost > 0 ? (stockPnL / stockCost) * 100 : 0;
    const optionPnLPct = optionCost > 0 ? (optionPnL / optionCost) * 100 : 0;

    const html = `
        <h3>${valuation.portfolioName} - Portfolio Valuation</h3>

        <!-- Summary Section -->
        <div class="detail-grid">
            <div class="detail-item">
                <div class="detail-label">Cash</div>
                <div class="detail-value">$${valuation.cash.toFixed(2)}</div>
            </div>
            <div class="detail-item">
                <div class="detail-label">Total Position Value</div>
                <div class="detail-value">$${valuation.totalPositionValue.toFixed(2)}</div>
            </div>
            <div class="detail-item">
                <div class="detail-label">Total Portfolio Value</div>
                <div class="detail-value">$${valuation.totalValue.toFixed(2)}</div>
            </div>
            <div class="detail-item">
                <div class="detail-label">Total Unrealized P&L</div>
                <div class="detail-value" style="color: ${valuation.totalUnrealizedPnL >= 0 ? '#059669' : '#dc2626'}">
                    $${valuation.totalUnrealizedPnL.toFixed(2)} (${valuation.totalPnLPercentage >= 0 ? '+' : ''}${valuation.totalPnLPercentage.toFixed(2)}%)
                </div>
            </div>
        </div>

        <!-- Stock Positions Section -->
        <h4><span class="asset-type-badge stock">Stock</span> Positions</h4>
        <div class="detail-grid">
            <div class="detail-item">
                <div class="detail-label">Stock Value</div>
                <div class="detail-value">$${valuation.stockPositionValue.toFixed(2)}</div>
            </div>
            <div class="detail-item">
                <div class="detail-label">Stock Cost</div>
                <div class="detail-value">$${stockCost.toFixed(2)}</div>
            </div>
            <div class="detail-item">
                <div class="detail-label">Stock P&L</div>
                <div class="detail-value" style="color: ${stockPnL >= 0 ? '#059669' : '#dc2626'}">
                    $${stockPnL.toFixed(2)} (${stockPnLPct >= 0 ? '+' : ''}${stockPnLPct.toFixed(2)}%)
                </div>
            </div>
        </div>
        ${stockPositions.length > 0 ? `
        <table>
            <thead>
                <tr>
                    <th>Stock</th>
                    <th>Quantity</th>
                    <th>Avg Cost</th>
                    <th>Current Price</th>
                    <th>Current Value</th>
                    <th>P&L</th>
                    <th>P&L %</th>
                </tr>
            </thead>
            <tbody>
                ${stockPositions.map(pos => `
                    <tr>
                        <td>${pos.stock ? pos.stock.ticker : 'Stock #' + pos.stockId}</td>
                        <td>${pos.netQuantity}</td>
                        <td>$${pos.averageCost.toFixed(2)}</td>
                        <td>$${pos.currentPrice.toFixed(2)}</td>
                        <td>$${pos.currentValue.toFixed(2)}</td>
                        <td style="color: ${pos.unrealizedPnL >= 0 ? '#059669' : '#dc2626'}">
                            $${pos.unrealizedPnL.toFixed(2)}
                        </td>
                        <td style="color: ${pos.pnLPercentage >= 0 ? '#059669' : '#dc2626'}">
                            ${pos.pnLPercentage >= 0 ? '+' : ''}${pos.pnLPercentage.toFixed(2)}%
                        </td>
                    </tr>
                `).join('')}
            </tbody>
        </table>
        ` : '<p>No stock positions</p>'}

        <!-- Option Positions Section -->
        <h4><span class="asset-type-badge option">Option</span> Positions</h4>
        <div class="detail-grid">
            <div class="detail-item">
                <div class="detail-label">Option Value</div>
                <div class="detail-value">$${valuation.optionPositionValue.toFixed(2)}</div>
            </div>
            <div class="detail-item">
                <div class="detail-label">Option Cost</div>
                <div class="detail-value">$${optionCost.toFixed(2)}</div>
            </div>
            <div class="detail-item">
                <div class="detail-label">Option P&L</div>
                <div class="detail-value" style="color: ${optionPnL >= 0 ? '#059669' : '#dc2626'}">
                    $${optionPnL.toFixed(2)} (${optionPnLPct >= 0 ? '+' : ''}${optionPnLPct.toFixed(2)}%)
                </div>
            </div>
        </div>
        ${optionPositions.length > 0 ? `
        <p class="greeks-subtitle">Greeks (summed across all option positions):</p>
        <div class="detail-grid">
            <div class="detail-item">
                <div class="detail-label">Delta (Δ)</div>
                <div class="detail-value">${valuation.portfolioDelta?.toFixed(4) || 'N/A'}</div>
            </div>
            <div class="detail-item">
                <div class="detail-label">Gamma (Γ)</div>
                <div class="detail-value">${valuation.portfolioGamma?.toFixed(4) || 'N/A'}</div>
            </div>
            <div class="detail-item">
                <div class="detail-label">Vega (ν)</div>
                <div class="detail-value">${valuation.portfolioVega?.toFixed(4) || 'N/A'}</div>
            </div>
            <div class="detail-item">
                <div class="detail-label">Theta (Θ)</div>
                <div class="detail-value">${valuation.portfolioTheta?.toFixed(4) || 'N/A'}</div>
            </div>
            <div class="detail-item">
                <div class="detail-label">Rho (ρ)</div>
                <div class="detail-value">${valuation.portfolioRho?.toFixed(4) || 'N/A'}</div>
            </div>
        </div>
        <table>
            <thead>
                <tr>
                    <th>Option</th>
                    <th>Quantity</th>
                    <th>Avg Cost</th>
                    <th>Current Price</th>
                    <th>Current Value</th>
                    <th>P&L</th>
                    <th>P&L %</th>
                </tr>
            </thead>
            <tbody>
                ${optionPositions.map(pos => `
                    <tr>
                        <td>#${pos.optionId}</td>
                        <td>${pos.netQuantity}</td>
                        <td>$${pos.averageCost.toFixed(2)}</td>
                        <td>$${pos.currentPrice.toFixed(2)}</td>
                        <td>$${pos.currentValue.toFixed(2)}</td>
                        <td style="color: ${pos.unrealizedPnL >= 0 ? '#059669' : '#dc2626'}">
                            $${pos.unrealizedPnL.toFixed(2)}
                        </td>
                        <td style="color: ${pos.pnLPercentage >= 0 ? '#059669' : '#dc2626'}">
                            ${pos.pnLPercentage >= 0 ? '+' : ''}${pos.pnLPercentage.toFixed(2)}%
                        </td>
                    </tr>
                `).join('')}
            </tbody>
        </table>
        ` : '<p>No option positions</p>'}
    `;

    container.innerHTML = html;
}

function updateDashboardFromValuation(valuation) {
    // Calculate separate stock and option P&L
    const stockPositions = valuation.positions.filter(p => p.assetType === 0);
    const optionPositions = valuation.positions.filter(p => p.assetType === 1);

    const stockPnL = stockPositions.reduce((sum, p) => sum + p.unrealizedPnL, 0);
    const optionPnL = optionPositions.reduce((sum, p) => sum + p.unrealizedPnL, 0);
    const stockCost = stockPositions.reduce((sum, p) => sum + p.totalCost, 0);
    const optionCost = optionPositions.reduce((sum, p) => sum + p.totalCost, 0);
    const stockPnLPct = stockCost > 0 ? (stockPnL / stockCost) * 100 : 0;
    const optionPnLPct = optionCost > 0 ? (optionPnL / optionCost) * 100 : 0;

    // Update total dashboard values
    document.getElementById('total-pnl').textContent = `$${valuation.totalUnrealizedPnL.toFixed(2)}`;
    document.getElementById('total-pnl').className = 'card-value ' +
        (valuation.totalUnrealizedPnL >= 0 ? 'positive' : 'negative');

    document.getElementById('pnl-percentage').textContent =
        `${valuation.totalPnLPercentage >= 0 ? '+' : ''}${valuation.totalPnLPercentage.toFixed(2)}%`;

    document.getElementById('total-value').textContent = `$${valuation.totalValue.toFixed(2)}`;
    document.getElementById('position-value').textContent = `$${valuation.totalPositionValue.toFixed(2)}`;
    document.getElementById('cash-balance').textContent = `$${valuation.cash.toFixed(2)}`;

    // Update stock P&L values
    document.getElementById('stock-pnl').textContent = `$${stockPnL.toFixed(2)}`;
    document.getElementById('stock-pnl').className = 'card-value ' + (stockPnL >= 0 ? 'positive' : 'negative');
    document.getElementById('stock-pnl-percentage').textContent =
        `${stockPnLPct >= 0 ? '+' : ''}${stockPnLPct.toFixed(2)}%`;
    document.getElementById('stock-value').textContent = `$${valuation.stockPositionValue.toFixed(2)}`;

    // Update option P&L values
    document.getElementById('option-pnl').textContent = `$${optionPnL.toFixed(2)}`;
    document.getElementById('option-pnl').className = 'card-value ' + (optionPnL >= 0 ? 'positive' : 'negative');
    document.getElementById('option-pnl-percentage').textContent =
        `${optionPnLPct >= 0 ? '+' : ''}${optionPnLPct.toFixed(2)}%`;
    document.getElementById('option-value').textContent = `$${valuation.optionPositionValue.toFixed(2)}`;

    // Update Greeks (only from options)
    if (optionPositions.length > 0) {
        document.getElementById('portfolio-greeks').style.display = 'block';
        document.getElementById('portfolio-delta').textContent =
            valuation.portfolioDelta?.toFixed(4) || 'N/A';
        document.getElementById('portfolio-gamma').textContent =
            valuation.portfolioGamma?.toFixed(4) || 'N/A';
        document.getElementById('portfolio-vega').textContent =
            valuation.portfolioVega?.toFixed(4) || 'N/A';
        document.getElementById('portfolio-theta').textContent =
            valuation.portfolioTheta?.toFixed(4) || 'N/A';
        document.getElementById('portfolio-rho').textContent =
            valuation.portfolioRho?.toFixed(4) || 'N/A';
    } else {
        document.getElementById('portfolio-greeks').style.display = 'none';
    }

    // Update P&L chart with new data
    updatePnLChart();
}

// ==================== P&L Chart ====================

function initializePnLChart() {
    const ctx = document.getElementById('pnl-chart');
    if (!ctx) return;

    // Check if Chart.js is loaded
    if (typeof Chart === 'undefined') {
        console.error('Chart.js not loaded yet');
        return;
    }

    // Create the chart
    pnlChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: [],
            datasets: [{
                label: 'Portfolio P&L',
                data: [],
                borderColor: '#4a90e2',
                backgroundColor: 'rgba(74, 144, 226, 0.1)',
                borderWidth: 2,
                fill: true,
                tension: 0.4,
                pointRadius: 0,
                pointHoverRadius: 6,
                pointHoverBackgroundColor: '#4a90e2',
                pointHoverBorderColor: '#ffffff',
                pointHoverBorderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                intersect: false,
                mode: 'index'
            },
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    enabled: true,
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#ffffff',
                    bodyColor: '#ffffff',
                    borderColor: '#4a90e2',
                    borderWidth: 1,
                    padding: 12,
                    displayColors: false,
                    callbacks: {
                        label: function(context) {
                            const value = context.parsed.y;
                            const prefix = value >= 0 ? '+$' : '-$';
                            return `P&L: ${prefix}${Math.abs(value).toFixed(2)}`;
                        }
                    }
                }
            },
            scales: {
                x: {
                    grid: {
                        color: 'rgba(0, 0, 0, 0.05)'
                    },
                    ticks: {
                        color: '#000000',
                        maxRotation: 45,
                        minRotation: 0
                    }
                },
                y: {
                    grid: {
                        color: 'rgba(0, 0, 0, 0.05)'
                    },
                    ticks: {
                        color: '#000000',
                        callback: function(value) {
                            return '$' + value.toFixed(0);
                        }
                    },
                    beginAtZero: true
                }
            }
        }
    });

    // Initialize time period toggles
    const toggles = document.querySelectorAll('.time-toggle');
    toggles.forEach(toggle => {
        toggle.addEventListener('click', function() {
            toggles.forEach(t => t.classList.remove('active'));
            this.classList.add('active');
            currentTimePeriod = this.dataset.period;
            updatePnLChart();
        });
    });

    // Initial chart update
    updatePnLChart();
}

function updatePnLChart() {
    // Check if chart is initialized
    if (!pnlChart) {
        return;
    }

    if (!currentPortfolioId) {
        // Show empty chart if no portfolio selected
        pnlChart.data.labels = [];
        pnlChart.data.datasets[0].data = [];
        pnlChart.update();
        return;
    }

    // Generate historical P&L data based on selected time period
    const data = generateHistoricalPnLData(currentTimePeriod, currentPortfolioId);

    pnlChart.data.labels = data.labels;
    pnlChart.data.datasets[0].data = data.values;
    pnlChart.update();
}

function generateHistoricalPnLData(period, portfolioId) {
    // Get portfolio creation date (for now, use a simulated date)
    const portfolio = portfolios.find(p => p.id == portfolioId);
    const creationDate = portfolio ? new Date(portfolio.createdAt) : new Date();
    const now = new Date();

    let startDate, dataPoints, labelFormat;

    switch(period) {
        case '1d':
            startDate = new Date(now - 24 * 60 * 60 * 1000);
            dataPoints = 24; // Hourly
            labelFormat = 'hour';
            break;
        case '1w':
            startDate = new Date(now - 7 * 24 * 60 * 60 * 1000);
            dataPoints = 7; // Daily
            labelFormat = 'day';
            break;
        case '1m':
            startDate = new Date(now - 30 * 24 * 60 * 60 * 1000);
            dataPoints = 30; // Daily
            labelFormat = 'day';
            break;
        case '1y':
            startDate = new Date(now - 365 * 24 * 60 * 60 * 1000);
            dataPoints = 12; // Monthly
            labelFormat = 'month';
            break;
        case '5y':
            startDate = new Date(now - 5 * 365 * 24 * 60 * 60 * 1000);
            dataPoints = 60; // Monthly
            labelFormat = 'month';
            break;
        default:
            startDate = new Date(now - 30 * 24 * 60 * 60 * 1000);
            dataPoints = 30;
            labelFormat = 'day';
    }

    const labels = [];
    const values = [];
    const timeStep = (now - startDate) / dataPoints;

    // Get current P&L from dashboard (if available)
    const currentPnLElement = document.getElementById('total-pnl');
    const currentPnLText = currentPnLElement?.textContent || '$0';
    const currentPnL = parseFloat(currentPnLText.replace(/[$,]/g, '')) || 0;

    for (let i = 0; i <= dataPoints; i++) {
        const date = new Date(startDate.getTime() + (timeStep * i));

        // If this date is before portfolio creation, P&L is 0
        let pnlValue = 0;
        if (date >= creationDate) {
            // Simulate historical P&L growth from 0 to current
            const timeSinceCreation = date - creationDate;
            const totalTimeSinceCreation = now - creationDate;
            const progressRatio = totalTimeSinceCreation > 0 ? timeSinceCreation / totalTimeSinceCreation : 0;

            // Add some random variation for realism
            const randomVariation = (Math.random() - 0.5) * 0.2; // +/- 10%
            pnlValue = currentPnL * progressRatio * (1 + randomVariation);
        }

        values.push(pnlValue);

        // Format label based on period
        let label;
        switch(labelFormat) {
            case 'hour':
                label = date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
                break;
            case 'day':
                label = date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
                break;
            case 'month':
                label = date.toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
                break;
            default:
                label = date.toLocaleDateString('en-US');
        }
        labels.push(label);
    }

    return { labels, values };
}

// ==================== Pricing History View ====================

async function loadPricingHistoryTable() {
    const tableContainer = document.getElementById('pricing-history-table');
    tableContainer.innerHTML = '<div class="loading">Loading pricing history...</div>';

    try {
        const response = await fetch(`${API_BASE}/api/pricing/history?limit=100`);
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }
        const history = await response.json();

        if (history.length === 0) {
            tableContainer.innerHTML = '<p>No pricing history found.</p>';
            return;
        }

        const simModeNames = ['Plain', 'Antithetic', 'Control Variate', 'Antithetic+Control', 'Van Der Corput'];

        const table = `
            <table>
                <thead>
                    <tr>
                        <th>Timestamp</th>
                        <th>Source</th>
                        <th>Option</th>
                        <th>Stock</th>
                        <th>Strike</th>
                        <th>Price</th>
                        <th>Vol</th>
                        <th>Steps</th>
                        <th>Paths</th>
                        <th>Mode</th>
                        <th>Delta</th>
                        <th>Gamma</th>
                    </tr>
                </thead>
                <tbody>
                    ${history.map(h => `
                        <tr>
                            <td>${new Date(h.timestamp).toLocaleString()}</td>
                            <td>${h.requestSource}</td>
                            <td>#${h.optionId} ${h.optionType || 'N/A'}</td>
                            <td>${h.stockSymbol || 'N/A'}</td>
                            <td>${h.strike ? h.strike.toFixed(2) : 'N/A'}</td>
                            <td>${h.price.toFixed(4)}</td>
                            <td>${(h.volatility * 100).toFixed(1)}%</td>
                            <td>${h.timeSteps}</td>
                            <td>${h.numberOfPaths.toLocaleString()}</td>
                            <td>${simModeNames[h.simMode] || h.simMode}</td>
                            <td>${h.delta ? h.delta.toFixed(4) : 'N/A'}</td>
                            <td>${h.gamma ? h.gamma.toFixed(4) : 'N/A'}</td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        `;

        tableContainer.innerHTML = table;
    } catch (error) {
        console.error('Error loading pricing history:', error);
        tableContainer.innerHTML = '<p class="error-box">Error loading pricing history</p>';
    }
}
