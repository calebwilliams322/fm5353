# Monte Carlo Option Pricer UI Documentation

This document explains all the HTML constructs, JavaScript objects, and CSS classes used in the web interface.

---

## File Structure

```
MonteCarloAPI/wwwroot/
├── index.html           # Main UI page
├── css/
│   └── styles.css      # All styling and visual design
└── js/
    └── app.js          # JavaScript logic and API integration
```

---

## HTML Structure (`index.html`)

### Document Structure

#### `<!DOCTYPE html>`
- Declares this as an HTML5 document
- Ensures modern browser rendering

#### `<html lang="en">`
- Root element of the HTML document
- `lang="en"` specifies the page is in English (helps screen readers and search engines)

#### `<head>` Section
Contains metadata and resource links:

```html
<meta charset="UTF-8">
```
- Sets character encoding to UTF-8 (supports all international characters)

```html
<meta name="viewport" content="width=device-width, initial-scale=1.0">
```
- Makes the page responsive on mobile devices
- `width=device-width` sets width to device screen width
- `initial-scale=1.0` sets initial zoom level to 100%

```html
<title>Monte Carlo Option Pricer</title>
```
- Text shown in browser tab

```html
<link rel="stylesheet" href="/css/styles.css">
```
- Links to external CSS file for styling
- `/css/styles.css` is an absolute path from web root

---

### `<body>` Section

The body contains all visible content, organized in a semantic structure:

---

## Main Container

```html
<div class="container">
```

**What it is:**
- A `<div>` (division) element is a generic container for grouping content
- `class="container"` applies CSS styling defined in `styles.css`

**Purpose:**
- Centers content on the page
- Provides consistent padding and maximum width
- Creates the main card-like appearance with white background

---

## Header Section

```html
<header>
    <h1>Monte Carlo Option Pricer</h1>
    <p>Price exotic options using Monte Carlo simulation</p>
</header>
```

**Elements:**

### `<header>`
- Semantic HTML5 element for introductory content
- Helps search engines and screen readers understand page structure

### `<h1>` (Heading 1)
- Main page title
- Most important heading on the page
- Only one `<h1>` per page is recommended for SEO

### `<p>` (Paragraph)
- Standard text paragraph element
- Contains subtitle/description

---

## Main Content Area

```html
<main>
```

**What it is:**
- Semantic HTML5 element for main content
- Tells browsers and assistive technology "this is the primary content"

**Contains:**
- Options List Section
- Pricing Form Section

---

## Section 1: Options List

```html
<section class="options-list">
    <h2>Stored Options</h2>
    <button id="loadOptions" class="btn btn-primary">Load Options</button>
    <div id="optionsList"></div>
</section>
```

### Elements Breakdown:

#### `<section class="options-list">`
- Semantic element for a thematic grouping of content
- `class="options-list"` applies specific CSS styling

#### `<h2>` (Heading 2)
- Second-level heading
- Indicates this section is about "Stored Options"

#### `<button id="loadOptions" class="btn btn-primary">`
- `<button>`: Interactive element users can click
- `id="loadOptions"`: Unique identifier
  - JavaScript uses this to attach click event listener
  - IDs must be unique on the page
- `class="btn btn-primary"`: Multiple CSS classes
  - `btn`: Base button styling
  - `btn-primary`: Primary button color (green)

#### `<div id="optionsList"></div>`
- Empty container that JavaScript fills dynamically
- `id="optionsList"`: JavaScript targets this to insert option cards
- Initially empty; populated when "Load Options" is clicked or page loads

---

## Section 2: Pricing Form

```html
<section class="pricing-section">
    <h2>Price an Option</h2>
    <form id="pricingForm">
        <!-- Form fields here -->
    </form>
    <div id="pricingResult" class="result-box"></div>
</section>
```

### `<form id="pricingForm">`

**What it is:**
- `<form>`: Container for user input elements
- `id="pricingForm"`: JavaScript uses this to handle form submission

**Attributes:**
- No `action` or `method` attributes (JavaScript handles submission via Fetch API)
- Prevents default browser form submission behavior

---

### Form Input Fields

Each form field follows this pattern:

```html
<div class="form-group">
    <label for="optionId">Option ID:</label>
    <input type="number" id="optionId" required>
</div>
```

#### Elements:

##### `<div class="form-group">`
- Groups label + input together
- `class="form-group"` applies spacing and layout styling

##### `<label for="optionId">`
- `<label>`: Descriptive text for an input field
- `for="optionId"`: Associates label with input by matching the input's `id`
- Clicking the label focuses the input field

##### `<input type="number" id="optionId" required>`
- `<input>`: User input field
- `type="number"`: Only allows numeric input
  - Displays number keyboard on mobile
  - Built-in validation
- `id="optionId"`: Unique identifier
  - JavaScript uses this to get the value
  - Must match the label's `for` attribute
- `required`: HTML5 validation attribute
  - Browser prevents submission if empty
  - Shows error message automatically

---

### Input Field Types Used

#### 1. **Number Input**
```html
<input type="number" id="initialPrice" value="100" step="0.01" required>
```

**Attributes:**
- `value="100"`: Default/pre-filled value
- `step="0.01"`: Allows decimal numbers with 2 decimal places
- Browser shows up/down arrows to increment/decrement

#### 2. **Checkbox Input**
```html
<label for="useMultithreading">
    <input type="checkbox" id="useMultithreading" checked>
    Use Multithreading
</label>
```

**Attributes:**
- `type="checkbox"`: Creates a checkable box
- `checked`: Checkbox is checked by default
- Label wraps the input (alternative valid HTML structure)

---

### Submit Button

```html
<button type="submit" class="btn btn-success">Price Option</button>
```

**Attributes:**
- `type="submit"`: Clicking triggers form submission
  - JavaScript intercepts this with `event.preventDefault()`
- `class="btn btn-success"`: Green success-style button

---

### Results Container

```html
<div id="pricingResult" class="result-box"></div>
```

**Purpose:**
- Empty container for displaying pricing results
- `id="pricingResult"`: JavaScript targets this to insert results
- `class="result-box"`: CSS styling for result display area
- Hidden until results are available (via CSS `.show` class)

---

## JavaScript Integration

```html
<script src="/js/app.js"></script>
```

**Placement:**
- At the end of `<body>` (after all HTML content)
- Ensures HTML is fully loaded before JavaScript runs

**Purpose:**
- Links external JavaScript file
- `/js/app.js` contains all interactive behavior

---

## JavaScript Objects and Functions (`app.js`)

### Global Constants

```javascript
const API_BASE = '';
```

**What it is:**
- `const`: Declares a constant (cannot be reassigned)
- Empty string means "same origin" (same domain as the HTML page)
- Used to build API endpoint URLs

**Example:**
```javascript
`${API_BASE}/api/options`  // Becomes: "/api/options"
```

---

### Event Listeners

#### 1. Load Options Button
```javascript
document.getElementById('loadOptions').addEventListener('click', loadOptions);
```

**Breakdown:**
- `document`: The entire HTML document object
- `.getElementById('loadOptions')`: Finds element with `id="loadOptions"`
- `.addEventListener('click', loadOptions)`:
  - When clicked, run the `loadOptions` function
  - `'click'`: Event type to listen for
  - `loadOptions`: Function to call (defined later)

#### 2. Form Submission
```javascript
document.getElementById('pricingForm').addEventListener('submit', priceOption);
```

**What it does:**
- Intercepts form submission (when "Price Option" button is clicked)
- Calls `priceOption` function instead of browser's default form submission

#### 3. Page Load Auto-Load
```javascript
window.addEventListener('DOMContentLoaded', loadOptions);
```

**Events:**
- `window`: The browser window object
- `'DOMContentLoaded'`: Fires when HTML is fully parsed
- Automatically loads options when page first loads

---

### Async Functions

JavaScript uses modern `async/await` syntax for API calls:

```javascript
async function loadOptions() { ... }
```

**What it means:**
- `async`: Function can use `await` keyword
- `await`: Pauses function until a Promise resolves
- Cleaner than callback or `.then()` syntax

---

## Function 1: `loadOptions()`

```javascript
async function loadOptions() {
    const optionsList = document.getElementById('optionsList');
    optionsList.innerHTML = '<div class="loading">Loading options...</div>';

    try {
        const response = await fetch(`${API_BASE}/api/options`);
        const data = await response.json();

        if (data.options && data.options.length > 0) {
            optionsList.innerHTML = data.options.map(opt => createOptionCard(opt)).join('');
        } else {
            optionsList.innerHTML = '<p>No options found in database.</p>';
        }
    } catch (error) {
        optionsList.innerHTML = `<div class="error-box">Error loading options: ${error.message}</div>`;
    }
}
```

### Step-by-Step:

#### 1. Get Container Element
```javascript
const optionsList = document.getElementById('optionsList');
```
- Finds the `<div id="optionsList">` element
- Stores reference in `optionsList` variable

#### 2. Show Loading Message
```javascript
optionsList.innerHTML = '<div class="loading">Loading options...</div>';
```
- `.innerHTML`: Property that sets HTML content inside element
- Replaces any existing content with loading message

#### 3. Fetch Data from API
```javascript
const response = await fetch(`${API_BASE}/api/options`);
```
- `fetch()`: Modern browser API for HTTP requests
- `await`: Waits for response before continuing
- Makes GET request to `/api/options` endpoint
- Returns Response object

#### 4. Parse JSON Response
```javascript
const data = await response.json();
```
- `.json()`: Parses response body as JSON
- `await`: Waits for parsing to complete
- `data` now contains JavaScript object with options array

#### 5. Process Results
```javascript
if (data.options && data.options.length > 0) {
    optionsList.innerHTML = data.options.map(opt => createOptionCard(opt)).join('');
}
```

**Array Methods Used:**

##### `.map()`
```javascript
data.options.map(opt => createOptionCard(opt))
```
- Transforms each option into HTML string
- `opt`: Current option object in iteration
- `=>`: Arrow function syntax
- Returns new array of HTML strings

##### `.join('')`
```javascript
.join('')
```
- Combines array of strings into single string
- `''`: Empty string separator (no separator between items)

#### 6. Error Handling
```javascript
try { ... } catch (error) { ... }
```
- `try`: Attempts to run code
- `catch`: Runs if any error occurs in `try` block
- `error`: Contains error information
- Prevents page from crashing on error

---

## Function 2: `createOptionCard(option)`

```javascript
function createOptionCard(option) {
    const optionTypeNames = ['European', 'Asian', 'Digital', 'Barrier', 'Lookback', 'Range'];
    const typeName = optionTypeNames[option.optionParameters.optionType] || 'Unknown';
    const callPut = option.optionParameters.isCall ? 'Call' : 'Put';

    return `
        <div class="option-card">
            <h3>Option #${option.id} - ${typeName} ${callPut}</h3>
            ...
        </div>
    `;
}
```

### Key Concepts:

#### 1. Array Lookup
```javascript
const optionTypeNames = ['European', 'Asian', 'Digital', 'Barrier', 'Lookback', 'Range'];
const typeName = optionTypeNames[option.optionParameters.optionType] || 'Unknown';
```
- Array index maps enum value to name
- `option.optionParameters.optionType`: Number (0-5)
- `|| 'Unknown'`: Default value if undefined

#### 2. Ternary Operator
```javascript
const callPut = option.optionParameters.isCall ? 'Call' : 'Put';
```
- Shorthand if/else statement
- **Syntax:** `condition ? valueIfTrue : valueIfFalse`
- If `isCall` is true, use "Call", otherwise "Put"

#### 3. Template Literals
```javascript
return `
    <div class="option-card">
        <h3>Option #${option.id} - ${typeName} ${callPut}</h3>
    </div>
`;
```
- Backticks `` ` `` instead of quotes
- `${variable}`: Embeds JavaScript expression
- Multiline strings without concatenation
- Creates HTML string dynamically

#### 4. JavaScript Date Formatting
```javascript
${new Date(option.createdAt).toLocaleDateString()}
```
- `new Date()`: Creates Date object from ISO string
- `.toLocaleDateString()`: Formats date based on user's locale
- Example output: "11/12/2025"

---

## Function 3: `priceOption(event)`

```javascript
async function priceOption(event) {
    event.preventDefault();

    // ... rest of function
}
```

### Event Object

#### `event.preventDefault()`
- `event`: Automatically passed by browser when event fires
- `.preventDefault()`: Stops default form submission behavior
  - Prevents page refresh
  - Allows JavaScript to handle submission via AJAX

### Gathering Form Data

```javascript
const optionId = document.getElementById('optionId').value;
const params = {
    initialPrice: parseFloat(document.getElementById('initialPrice').value),
    volatility: parseFloat(document.getElementById('volatility').value),
    // ...
};
```

#### `.value` Property
- Gets the current value from input field
- Returns string for text/number inputs
- Returns boolean for checkboxes (checked/unchecked)

#### Type Conversion Functions

##### `parseFloat()`
```javascript
parseFloat(document.getElementById('initialPrice').value)
```
- Converts string to floating-point number
- Example: `"100.50"` → `100.5`

##### `parseInt()`
```javascript
parseInt(document.getElementById('timeSteps').value)
```
- Converts string to integer
- Example: `"252"` → `252`

#### Checkbox Value
```javascript
useMultithreading: document.getElementById('useMultithreading').checked
```
- `.checked`: Returns `true` or `false` for checkbox state
- Not `.value` for checkboxes!

---

### Making POST Request

```javascript
const response = await fetch(`${API_BASE}/api/pricing/${optionId}`, {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json'
    },
    body: JSON.stringify(params)
});
```

#### Fetch Options Object

##### `method: 'POST'`
- HTTP method (GET, POST, PUT, DELETE, etc.)
- POST sends data to server

##### `headers: { 'Content-Type': 'application/json' }`
- HTTP headers sent with request
- Tells server we're sending JSON data

##### `body: JSON.stringify(params)`
- `JSON.stringify()`: Converts JavaScript object to JSON string
- Example: `{initialPrice: 100}` → `'{"initialPrice":100}'`
- Required format for sending data in request body

---

### Response Handling

```javascript
if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'Pricing failed');
}

const data = await response.json();
displayPricingResult(data);
```

#### `response.ok`
- Boolean property
- `true` if status code is 200-299
- `false` for 400, 500 errors, etc.

#### `throw new Error()`
- Creates and throws an error
- Jumps to `catch` block
- Stops execution of current code path

---

## Function 4: `displayPricingResult(data)`

```javascript
function displayPricingResult(data) {
    const result = data.pricingResult;
    const option = data.option;

    const html = `
        <h3>Pricing Result</h3>
        <div style="margin-bottom: 20px;">
            <strong>Option Price:</strong>
            <span style="font-size: 1.5em; color: #22543d;">$${result.price.toFixed(4)}</span>
        </div>
        ...
    `;

    const resultBox = document.getElementById('pricingResult');
    resultBox.innerHTML = html;
}
```

### JavaScript Number Methods

#### `.toFixed(4)`
```javascript
result.price.toFixed(4)
```
- Formats number to fixed decimal places
- Example: `123.456789` → `"123.4568"`
- Returns string, not number
- Rounds to specified precision

### Null Coalescing

```javascript
${result.standardError?.toFixed(6) || 'N/A'}
```

#### Optional Chaining (`?.`)
- `result.standardError?.toFixed(6)`
- If `standardError` is `null` or `undefined`, returns `undefined`
- Prevents "Cannot read property of undefined" errors

#### Logical OR (`||`)
- `value || 'N/A'`
- If `value` is falsy (null, undefined, 0, false, ''), use `'N/A'`
- Provides default/fallback value

---

## CSS Classes (`styles.css`)

### Layout Classes

#### `.container`
```css
.container {
    max-width: 1000px;
    margin: 0 auto;
    padding: 40px 20px;
    background-color: white;
    border-radius: 8px;
}
```

**Properties:**
- `max-width`: Maximum width (content won't stretch beyond this)
- `margin: 0 auto`: Centers element horizontally
  - `0`: Top/bottom margin
  - `auto`: Left/right margin (auto-calculates to center)
- `padding`: Space inside element (40px top/bottom, 20px left/right)
- `border-radius`: Rounded corners

---

#### `.form-group`
```css
.form-group {
    margin-bottom: 15px;
}
```
- Adds vertical spacing between form fields

---

### Button Classes

#### `.btn` (Base Button Style)
```css
.btn {
    padding: 10px 20px;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-size: 16px;
    transition: background-color 0.3s ease;
}
```

**Properties:**
- `cursor: pointer`: Shows hand cursor on hover
- `transition`: Smooth color change animation (0.3 seconds)

#### `.btn-primary`
```css
.btn-primary {
    background-color: #4299e1;
    color: white;
}
```
- Blue button (Load Options button)

#### `.btn-success`
```css
.btn-success {
    background-color: #48bb78;
    color: white;
}
```
- Green button (Price Option button)

---

### Card Classes

#### `.option-card`
```css
.option-card {
    border: 1px solid #e2e8f0;
    padding: 20px;
    margin-bottom: 15px;
    border-radius: 8px;
    background-color: #f7fafc;
}
```
- Visual container for each option
- Light gray background
- Subtle border and shadow

---

### Grid Layout

#### `.greeks-grid`
```css
.greeks-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
    gap: 15px;
}
```

**CSS Grid Properties:**
- `display: grid`: Enables CSS Grid layout
- `grid-template-columns`: Defines column structure
  - `repeat()`: Repeats pattern
  - `auto-fit`: Creates as many columns as fit
  - `minmax(150px, 1fr)`: Each column is 150px-1fr wide
  - `1fr`: One fraction of available space
- `gap: 15px`: Space between grid items

**Result:** Responsive grid that adjusts columns based on screen width

---

### Responsive Design

#### Media Query
```css
@media (max-width: 768px) {
    .container {
        padding: 20px 10px;
    }
}
```

**What it does:**
- `@media`: Conditional CSS rules
- `(max-width: 768px)`: Only applies when screen ≤ 768px
- Adjusts layout for mobile/tablet devices

---

## Data Flow Diagram

```
User Action → JavaScript Function → Fetch API → ASP.NET API → Database
                                                      ↓
User sees result ← JavaScript displays ← JSON response ← API returns data
```

### Example: Loading Options

1. **User clicks "Load Options"**
2. **JavaScript:** `loadOptions()` function runs
3. **Browser:** `fetch('/api/options')` makes HTTP GET request
4. **API:** `OptionsController.GetAllOptions()` queries database
5. **Database:** Returns option records
6. **API:** Serializes to JSON and returns
7. **Browser:** Receives JSON response
8. **JavaScript:** Parses JSON, creates HTML cards
9. **Browser:** Displays option cards on page

---

## Common JavaScript Patterns Used

### 1. Arrow Functions
```javascript
// Traditional function
function add(a, b) {
    return a + b;
}

// Arrow function
const add = (a, b) => a + b;
```

### 2. Template Literals
```javascript
// Old way
const message = "Hello, " + name + "!";

// Template literal
const message = `Hello, ${name}!`;
```

### 3. Destructuring
```javascript
// Without destructuring
const price = data.pricingResult.price;
const delta = data.pricingResult.delta;

// With destructuring
const { price, delta } = data.pricingResult;
```

### 4. Async/Await
```javascript
// Old way (callbacks)
fetch('/api/options').then(response => {
    return response.json();
}).then(data => {
    console.log(data);
});

// Modern way
const response = await fetch('/api/options');
const data = await response.json();
console.log(data);
```

---

## API Endpoints Used

### GET `/api/options`
**Purpose:** Retrieve all stored options

**Response:**
```json
{
    "count": 2,
    "options": [
        {
            "id": 1,
            "optionParameters": {
                "strike": 100,
                "optionType": 0,
                "isCall": true
            },
            "createdAt": "2025-11-12T00:00:00Z"
        }
    ]
}
```

### POST `/api/pricing/{optionId}`
**Purpose:** Price a specific option

**Request Body:**
```json
{
    "initialPrice": 100.0,
    "volatility": 0.2,
    "riskFreeRate": 0.05,
    "timeToExpiry": 1.0,
    "timeSteps": 252,
    "numberOfPaths": 10000,
    "useMultithreading": true,
    "simMode": 0
}
```

**Response:**
```json
{
    "option": { ... },
    "pricingResult": {
        "price": 12.3456,
        "standardError": 0.123456,
        "delta": 0.6234,
        "gamma": 0.0123,
        "vega": 0.3456,
        "theta": -0.0234,
        "rho": 0.5123,
        "simulationMode": "Plain"
    },
    "simulationParameters": { ... }
}
```

---

## Key Concepts Summary

### HTML
- **Semantic elements**: `<header>`, `<main>`, `<section>` provide meaning
- **Forms**: Group input fields with validation
- **IDs vs Classes**: IDs are unique, classes can be reused
- **Labels**: Associate descriptive text with inputs

### JavaScript
- **DOM manipulation**: `document.getElementById()`, `.innerHTML`
- **Event handling**: `.addEventListener()` responds to user actions
- **Async programming**: `async`/`await` for API calls
- **Fetch API**: Modern way to make HTTP requests
- **Template literals**: Clean string interpolation with `${}`

### CSS
- **Flexbox/Grid**: Modern layout systems
- **Responsive design**: Media queries adapt to screen size
- **Transitions**: Smooth animations on property changes
- **Classes**: Reusable styling rules

---

## Next Steps / Future Enhancements

Potential improvements to the UI:

1. **Form Validation**
   - Add min/max constraints to inputs
   - Show validation errors inline

2. **Loading States**
   - Disable button during API calls
   - Show spinner animations

3. **Error Handling**
   - More detailed error messages
   - Retry mechanism for failed requests

4. **Data Visualization**
   - Charts for price paths
   - Greek sensitivity graphs

5. **Option Creation**
   - Add form to create new options
   - Delete/edit existing options

6. **History View**
   - Display pricing history table
   - Compare results over time

---

## Troubleshooting

### UI Not Loading
- Check browser console for errors (F12 → Console tab)
- Verify API is running (`dotnet run`)
- Confirm files exist in `wwwroot/` folder

### API Calls Failing
- Check Network tab in browser dev tools
- Verify endpoint URLs match controller routes
- Check CORS is enabled in `Program.cs`

### Styling Issues
- Clear browser cache (Ctrl+Shift+R)
- Verify `styles.css` path is correct
- Check for CSS syntax errors

---

## Resources for Learning More

### HTML/CSS
- [MDN Web Docs](https://developer.mozilla.org/)
- [CSS-Tricks](https://css-tricks.com/)

### JavaScript
- [JavaScript.info](https://javascript.info/)
- [MDN JavaScript Guide](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide)

### Web APIs
- [Fetch API](https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API)
- [DOM Events](https://developer.mozilla.org/en-US/docs/Web/Events)

---

*Last Updated: November 12, 2025*
