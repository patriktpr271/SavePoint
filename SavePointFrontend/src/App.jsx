import './App.css'
import React, { useEffect, useState } from 'react';


function App() {
  const [forecasts, setForecasts] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Adjust the port if your backend runs on a different one
    fetch('/api/WeatherForecast')
      .then(response => {
        if (!response.ok) {
          throw new Error("Failed to fetch weather data");
        }
        return response.json();
      })
      .then(data => {
        setForecasts(data);
        setLoading(false);
      })
      .catch(error => {
        console.error('Error fetching weather:', error);
        setLoading(false);
      });
  }, []);

  return (
    <div style={{ padding: '1rem' }}>
      <h1>Weather Forecast</h1>
      {loading ? (
        <p>Loading...</p>
      ) : (
        forecasts.map((item, index) => (
          <div key={index} style={{
            border: '1px solid #ccc',
            padding: '10px',
            marginBottom: '10px',
            borderRadius: '5px'
          }}>
            <p><strong>Date:</strong> {item.date}</p>
            <p><strong>Temperature (°C):</strong> {item.temperatureC}</p>
            <p><strong>Summary:</strong> {item.summary}</p>
          </div>
        ))
      )}
    </div>
  );
}

export default App;
