import './App.css';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import WeatherPage from './pages/weatherpage';
import VideogamesPage from './pages/VideogamesPage';
import Navbar from './components/navbar';
import HomeScreen from './components/HomeScreen';


function App() {
  return (
    <div className="w-full min-h-screen bg-base-200">
      <Router>
        <Navbar/>
        <Routes>
          <Route path="/" element={<HomeScreen />} />
          <Route path="/videogames" element={<VideogamesPage />} />
          <Route path="/weather" element={<WeatherPage />} />
        </Routes>
      </Router>
    </div>
  );
}

export default App;
