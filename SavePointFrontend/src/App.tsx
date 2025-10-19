import './App.css';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import VideogamesPage from './pages/VideogamesPage';
import UserProfilePage from './pages/UserProfilePage';
import UserListsPage from './pages/UserListsPage';
import ListDetailPage from './pages/ListDetailPage';
import Navbar from './components/navbar';
import HomeScreen from './components/HomeScreen';
import { AuthProvider } from './contexts/AuthContext';
import { ErrorProvider } from './contexts/ErrorContext';
import GlobalErrorDisplay from './components/GlobalErrorDisplay';


function App() {
  return (
    <ErrorProvider>
      <AuthProvider>
        <div className="w-full min-h-screen bg-base-200 text-base-content">
          <Router>
            <Navbar/>
            <Routes>
              <Route path="/" element={<HomeScreen />} />
              <Route path="/videogames" element={<VideogamesPage />} />
              <Route path="/profile" element={<UserProfilePage />} />
              <Route path="/lists" element={<UserListsPage />} />
              <Route path="/lists/:id" element={<ListDetailPage />} />
            </Routes>
          </Router>
          <GlobalErrorDisplay />
        </div>
      </AuthProvider>
    </ErrorProvider>
  );
}

export default App;
