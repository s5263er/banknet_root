import React, { useState, useEffect } from 'react';
import Plot from 'react-plotly.js';
import './StocksPage.css'; // Import your CSS file for styling

function StocksPage() {
  // Define state variables
  const [stockData, setStockData] = useState([]);
  const [stockSymbol, setStockSymbol] = useState('AAPL'); // Set the initial stock symbol
  const [longName, setLongName] = useState('');
  const [peRatio, setPeRatio] = useState('');
  const [earningsGrowthRate, setEarningsGrowthRate] = useState('');
  const [profitMargin, setProfitMargin] = useState('');
  const [debtToEquityRatio, setDebtToEquityRatio] = useState('');
  const [currentRatio, setCurrentRatio] = useState('');
  const [buy, setBuy] = useState('');
  const [strongBuy, setStrongBuy] = useState('');
  const [sell, setSell] = useState('');
  const [hold, setHold] = useState('');
  const [strongSell, setStrongSell] = useState('');
  const [metricInfo, setMetricInfo] = useState('');

  useEffect(() => {
    fetchStockData();
    fetchData();
  }, [stockSymbol]);

  const fetchStockData = async () => {
    try {
      // Fetch stock data from your backend API
      const token = localStorage.getItem('jwtToken');
      const headers = {
        Authorization: `Bearer ${token}`,
      };

      // Make a GET request to your backend API using the correct URL
      const response = await fetch(`http://localhost:5062/api/stock-data?symbol=${stockSymbol}`, {
        headers,
      });

      if (response.ok) {
        const data = await response.json();
        console.log(data);

        // Check if data is an array with at least one element
        if (Array.isArray(data) && data.length > 0) {
          setStockData(data);
        } else {
          console.error('Invalid stock data format');
        }
      } else {
        console.error('Failed to fetch stock data');
      }
    } catch (error) {
      console.error('Error fetching stock data:', error);
    }
  };

  async function fetchData() {
    const url = `https://apidojo-yahoo-finance-v1.p.rapidapi.com/stock/v2/get-analysis?symbol=${stockSymbol}&region=US`;
    const options = {
      method: 'GET',
      headers: {
        'X-RapidAPI-Key': 'ac645fda57mshace61460c023f9bp1af813jsnf00735905a68',
        'X-RapidAPI-Host': 'apidojo-yahoo-finance-v1.p.rapidapi.com'
      },
    };

    try {
      const response = await fetch(url, options);
      const result = await response.text();

      if (result) {
        // Parse the result as JSON
        const resultObject = JSON.parse(result);

        // Check if recommendationTrend and trend properties exist
        if (resultObject.recommendationTrend && resultObject.recommendationTrend.trend) {
          const trend = resultObject.recommendationTrend.trend;
          const latestTrend = trend[0]; // Assuming the latest trend is at index 0

          console.log("Latest Trend:", latestTrend);

          setStrongBuy(latestTrend.strongBuy);
          setBuy(latestTrend.buy);
          setHold(latestTrend.hold);
          setSell(latestTrend.sell);
          setStrongSell(latestTrend.strongSell);
        }

        // Check if other properties exist before accessing them
        if (resultObject.longName) {
          console.log("Long Name:", resultObject.longName);
          setLongName(resultObject.longName);
        }

        if (resultObject.indexTrend && resultObject.indexTrend.peRatio) {
          console.log("P/E Ratio:", resultObject.indexTrend.peRatio.fmt);
          setPeRatio(resultObject.indexTrend.peRatio.fmt);
        }

        if (resultObject.earningsHistory && resultObject.earningsHistory.history[0] && resultObject.earningsHistory.history[0].epsActual) {
          console.log("Earnings Growth Rate:", resultObject.earningsHistory.history[0].epsActual.fmt);
          setEarningsGrowthRate(resultObject.earningsHistory.history[0].epsActual.fmt);
        }

        if (resultObject.financialData && resultObject.financialData.profitMargins) {
          console.log("Profit Margin:", resultObject.financialData.profitMargins.fmt);
          setProfitMargin(resultObject.financialData.profitMargins.fmt);
        }

        if (resultObject.financialData && resultObject.financialData.debtToEquity) {
          console.log("Debt to Equity Ratio:", resultObject.financialData.debtToEquity.fmt);
          setDebtToEquityRatio(resultObject.financialData.debtToEquity.fmt);
        }
        if (resultObject.financialData && resultObject.financialData.currentRatio) {
          console.log("Current Ratio:", resultObject.financialData.currentRatio.fmt);
          setCurrentRatio(resultObject.financialData.currentRatio.fmt);
        }

        // Continue with the rest of your code
      } else {
        console.error("Result is empty.");
      }
    } catch (error) {
      console.error("Error:", error);
    }
  }

  // Extract timestamps, close prices, and formatted dates for hover text
  const timestamps = stockData.map(dataPoint => dataPoint.timestamp * 1000);
  const closePrices = stockData.map(dataPoint => dataPoint.closePrice);
  const dates = stockData.map(dataPoint => new Date(dataPoint.timestamp * 1000));

  const formattedDates = stockData.map(dataPoint => {
    const date = new Date(dataPoint.timestamp * 1000);
    return `${date.getDate()} ${date.toLocaleString('default', { month: 'short' })} ${date.getFullYear()}`;
  });

  // Placeholder variables (replace with actual values)

  return (
    <div className="stocks-page">
      <h1 className="page-title">{longName}</h1>

      {/* Stock Price Chart */}
      <div className="stock-chart">
        {timestamps.length > 0 ? (
          <Plot
            data={[
              {
                x: dates,
                y: closePrices,
                type: 'scatter',
                mode: 'lines',
                line: { color: 'blue', width: 2 },
                fill: 'tozeroy',
                fillcolor: 'rgba(0, 0, 255, 0.2)',
                hovertext: formattedDates,
              },
            ]}
            layout={{
              width: 720,
              height: 440,
              title: `${stockSymbol} Stock Price (Last 5 Years)`,
              xaxis: {
                title: 'Date',
                showgrid: true,
                gridcolor: 'rgba(0, 0, 0, 0.1)',
              },
              yaxis: {
                title: 'Close Price',
                showgrid: true,
                gridcolor: 'rgba(0, 0, 0, 0.1)',
              },
            }}
          />
        ) : (
          <p>Loading stock data...</p>
        )}
      </div>

      <div className="financial-metrics">
        {/* P/E Ratio */}
        <div className="metric-box" onClick={() => setMetricInfo("P/E Ratio: Indicates the price-to-earnings ratio of the stock.")}>
          <div className="metric-label">P/E Ratio</div>
          <div className="metric-value">{peRatio}</div>
        </div>

        {/* Earnings Growth Rate */}
        <div className="metric-box" onClick={() => setMetricInfo("Earnings Growth Rate: Indicates the rate of growth of earnings.")}>
          <div className="metric-label">Earnings Growth Rate</div>
          <div className="metric-value">{earningsGrowthRate}</div>
        </div>

        {/* Profit Margin */}
        <div className="metric-box" onClick={() => setMetricInfo("Profit Margin: Indicates the profit margin of the company.")}>
          <div className="metric-label">Profit Margin</div>
          <div className="metric-value">{profitMargin}</div>
        </div>

        {/* Debt to Equity Ratio */}
        <div className="metric-box" onClick={() => setMetricInfo("Debt to Equity Ratio: Indicates the ratio of debt to equity in the company.")}>
          <div className="metric-label">Debt to Equity Ratio</div>
          <div className="metric-value">{debtToEquityRatio}</div>
        </div>

        {/* Current Ratio */}
        <div className="metric-box" onClick={() => setMetricInfo("Current Ratio: Indicates the current ratio of the company.")}>
          <div className="metric-label">Current Ratio</div>
          <div className="metric-value">{currentRatio}</div>
        </div>
      </div>

      <div className="stock-info">
        {/* Recommendations */}
        <div className="recommendations">
          {/* Strong Buy */}
          <div className="recommendation strong-buy">
            <div className="recommendation-label"style={{ color: 'darkblue' }}>Strong Buy</div>
            <div className="recommendation-value" >{strongBuy}</div>
          </div>
          {/* Buy */}
          <div className="recommendation buy">
            <div className="recommendation-label"style={{ color: 'blue' }}>Buy</div>
            <div className="recommendation-value" >{buy}</div>
          </div>
          {/* Hold */}
          <div className="recommendation hold">
            <div className="recommendation-label">Hold</div>
            <div className="recommendation-value">{hold}</div>
          </div>
          {/* Sell */}
          <div className="recommendation sell">
            <div className="recommendation-label"style={{ color: '#FF5733' }}>Sell</div>
            <div className="recommendation-value" >{sell}</div>
          </div>
          {/* Strong Sell */}
          <div className="recommendation strong-sell">
            <div className="recommendation-label"style={{ color: '#FF0000' }}>Strong Sell</div>
            <div className="recommendation-value" >{strongSell}</div>
          </div>
        </div>

        {/* Metric Info */}
        <div className="metric-info">{metricInfo}</div>
      </div>
    </div>
  );
}

export default StocksPage;
