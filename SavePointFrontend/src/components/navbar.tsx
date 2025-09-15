import { useState } from "react";
import { Menu, X, Search } from "lucide-react";
import RegisterModal from "./RegisterModal";

export default function Navbar() {
  const [isOpen, setIsOpen] = useState(false);
  const [isRegisterModalOpen, setIsRegisterModalOpen] = useState(false);

  return (
    <div className="fixed top-0 left-0 w-full bg-base-100 shadow-md z-50">
      {/* Full width container with padding inside */}
      <div className="w-full px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          {/* Left - Logo */}
          <div className="flex-shrink-0">
            <a className="text-2xl font-bold">SavePoint</a>
          </div>

          {/* Center - Menu (desktop only) */}
          <nav className="hidden md:flex flex-1 justify-center space-x-8">
            <a className="hover:underline cursor-pointer">Sign In</a>
            <a className="hover:underline cursor-pointer">Lists</a>
            <a className="hover:underline cursor-pointer" onClick={() => setIsRegisterModalOpen(true)}>Create Account</a>
            <a className="hover:underline cursor-pointer">Videogames</a>
          </nav>

          {/* Right - Search (desktop only)  */}
          <div className="hidden md:flex items-center gap-4">
            <div className="input-group">
              <input
                type="text"
                placeholder="Search..."
                className="input input-bordered w-40 md:w-56"
              />
            </div>
          </div>

          {/* Mobile Menu Button */}
          <div className="flex items-center md:hidden">
            <button
              onClick={() => setIsOpen(!isOpen)}
              className="text-gray-700 focus:outline-none"
            >
              {isOpen ? <X size={24} /> : <Menu size={24} />}
            </button>
          </div>
        </div>
      </div>

      {/* Mobile Dropdown */}
      {isOpen && (
        <div className="md:hidden bg-base-200 px-4 pt-2 pb-4 space-y-2 shadow-inner">
          <a className="block hover:underline">Sign In</a>
          <a className="block hover:underline">Lists</a>
          <a className="block hover:underline" onClick={() => setIsRegisterModalOpen(true)}>Create Account</a>
          <a className="block hover:underline">Videogames</a>
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
      />
    </div>
  );
}
