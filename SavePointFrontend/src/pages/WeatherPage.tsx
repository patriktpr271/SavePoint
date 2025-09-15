import { useEffect, useState } from "react";

interface WeatherForecast {
  date: string;
  temperatureC: number;
  summary: string;
}

export default function WeatherPage() {
  const [forecasts, setForecasts] = useState<WeatherForecast[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetch("/api/WeatherForecast")
      .then((res) => res.json())
      .then((data) => {
        setForecasts(data);
        setLoading(false);
      })
      .catch((err) => {
        console.error("Error fetching weather:", err);
        setLoading(false);
      });
  }, []);

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <div className="p-6">Loading weather data...</div>
      </div>
    );
  }

  return (
    <div className="flex items-center justify-center min-h-[60vh]">
      <div className="p-6 w-full max-w-2xl">
        <h1 className="text-2xl font-bold mb-4 text-center">Weather Forecast</h1>
        <div className="overflow-x-auto">
          <table className="table w-full">
            <thead>
              <tr>
                <th>Date</th>
                <th>Temperature (°C)</th>
                <th>Summary</th>
              </tr>
            </thead>
            <tbody>
              {forecasts.map((f, idx) => (
                <tr key={idx}>
                  <td>{f.date}</td>
                  <td>{f.temperatureC}</td>
                  <td>{f.summary}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
