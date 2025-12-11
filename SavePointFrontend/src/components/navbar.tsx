import { useState } from "react";
import { Link } from "react-router-dom";
import { Menu, X, Search, User } from "lucide-react";
import RegisterModal from "./RegisterModal";
import LoginModal from "./LoginModal";
import { useAuth } from "../contexts/AuthContext";

export default function Navbar() {
  const [isOpen, setIsOpen] = useState(false);
  const [isRegisterModalOpen, setIsRegisterModalOpen] = useState(false);
  const [isLoginModalOpen, setIsLoginModalOpen] = useState(false);
  const { user, logout, loading } = useAuth();

  const handleLoginSuccess = (userData: any) => {
  };

  const handleLogout = async () => {
    await logout();
    setIsOpen(false);
  };

  const handleSwitchToRegister = () => {
    setIsLoginModalOpen(false);
    setIsRegisterModalOpen(true);
  };

  const handleSwitchToLogin = () => {
    setIsRegisterModalOpen(false);
    setIsLoginModalOpen(true);
  };

  if (loading) {
    return (
      <div className="fixed top-0 left-0 w-full bg-base-100 shadow-md z-50">
        <div className="w-full px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            <div className="flex-shrink-0">
              <Link to="/" className="text-2xl font-bold bg-gradient-to-r from-emerald-500 to-orange-500 bg-clip-text text-transparent hover:from-emerald-600 hover:to-orange-600 transition-all">SavePoint</Link>
            </div>
            <div className="loading loading-spinner loading-sm"></div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="fixed top-0 left-0 w-full bg-base-100 shadow-md z-50">
      {/* Full width container with padding inside */}
      <div className="w-full px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          {/* Left - Logo */}
          <div className="flex-shrink-0">
            <Link to="/" className="text-2xl font-bold bg-gradient-to-r from-primary to-secondary bg-clip-text text-transparent hover:from-emerald-600 hover:to-orange-600 transition-all">SavePoint</Link>
          </div>

          {/* Center - Menu (desktop only) */}
          <nav className="hidden md:flex flex-1 justify-center space-x-8">
            {user ? (
              <>
                <span className="text-base-content">Welcome, {user.displayName}!</span>
                <Link to="/profile" className="hover:underline cursor-pointer flex items-center gap-1">
                  <User size={16} />
                  Profile
                </Link>
                <Link to="/lists" className="hover:underline cursor-pointer">Lists</Link>
                {user.roles.includes('Admin') && (
                  <a className="hover:underline cursor-pointer text-warning">Admin Panel</a>
                )}
                <a className="hover:underline cursor-pointer" onClick={handleLogout}>Sign Out</a>
              </>
            ) : (
              <>
                <a className="hover:underline cursor-pointer" onClick={() => setIsLoginModalOpen(true)}>Sign In</a>
                <Link to="/lists" className="hover:underline cursor-pointer">Lists</Link>
                <a className="hover:underline cursor-pointer" onClick={() => setIsRegisterModalOpen(true)}>Create Account</a>
              </>
            )}
            <Link to="/videogames" className="hover:underline cursor-pointer">Videogames</Link>
          </nav>

          {/* Right - Search (desktop only)  */}
          <div className="hidden md:flex items-center gap-4">
            <div className="input-group">
              <input
                type="text"
                placeholder="Search..."
                className="input input-bordered w-40 md:w-56 bg-base-200 text-base-content border-base-300 focus:border-primary"
              />
            </div>
          </div>

          {/* Mobile Menu Button */}
          <div className="flex items-center md:hidden">
            <button
              onClick={() => setIsOpen(!isOpen)}
              className="text-base-content focus:outline-none"
            >
              {isOpen ? <X size={24} /> : <Menu size={24} />}
            </button>
          </div>
        </div>
      </div>

      {/* Mobile Dropdown */}
      {isOpen && (
        <div className="md:hidden bg-base-200 px-4 pt-2 pb-4 space-y-2 shadow-inner">
          {user ? (
            <>
              <div className="text-base-content font-medium">Welcome, {user.displayName}!</div>
              <Link to="/profile" className="block hover:underline flex items-center gap-2" onClick={() => setIsOpen(false)}>
                <User size={16} />
                Profile
              </Link>
              <Link to="/lists" className="block hover:underline" onClick={() => setIsOpen(false)}>Lists</Link>
              {user.roles.includes('Admin') && (
                <a className="block hover:underline text-warning">Admin Panel</a>
              )}
              <a className="block hover:underline" onClick={handleLogout}>Sign Out</a>
            </>
          ) : (
            <>
              <a className="block hover:underline" onClick={() => { setIsLoginModalOpen(true); setIsOpen(false); }}>Sign In</a>
              <Link to="/lists" className="block hover:underline" onClick={() => setIsOpen(false)}>Lists</Link>
              <a className="block hover:underline" onClick={() => { setIsRegisterModalOpen(true); setIsOpen(false); }}>Create Account</a>
            </>
          )}
          <Link to="/videogames" className="block hover:underline" onClick={() => setIsOpen(false)}>Videogames</Link>
          <div className="mt-3 flex gap-2 items-center">
            <input
              type="text"
              placeholder="Search..."
              className="input input-bordered w-full"
            />
          </div>
        </div>  
      )}
      
      <RegisterModal 
        isOpen={isRegisterModalOpen}
        onClose={() => setIsRegisterModalOpen(false)}
        onSwitchToLogin={handleSwitchToLogin}
      />
      
      <LoginModal 
        isOpen={isLoginModalOpen}
        onClose={() => setIsLoginModalOpen(false)}
        onLoginSuccess={handleLoginSuccess}
        onSwitchToRegister={handleSwitchToRegister}
      />
    </div>
  );
}
