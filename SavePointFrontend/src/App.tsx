import './App.css';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import WeatherPage from './pages/weatherpage';
import Navbar from './components/navbar';


function App() {
  return (
    <Router>
      <Navbar/>
      <Routes>
        <Route path="/" element={<h1>Welcome to SavePoint</h1>} />
        <Route path="/weather" element={<WeatherPage />} />
      </Routes>
    </Router>
  );
}

export default App;
