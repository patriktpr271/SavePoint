import React from 'react';

interface SidebarProps {
  className?: string;
}

const Sidebar: React.FC<SidebarProps> = ({ className = "" }) => {
  return (
    <aside className={`fixed top-20 left-0 h-[calc(100vh-5rem)] w-64 xl:w-80 bg-base-200 border-r border-base-300 z-40 hidden lg:block ${className}`}>
      <div className="p-6 h-full overflow-y-auto">
        <div className="text-lg font-semibold text-base-content mb-6 pb-2 border-b border-base-300">
          Filters & Categories
        </div>
        
        {/* Genre Filter Section */}
        <div className="mb-6">
          <h3 className="text-sm font-semibold text-base-content mb-3">Genres</h3>
          <div className="text-sm text-base-content/60 italic">
            Coming soon...
          </div>
        </div>

        {/* Platform Filter Section */}
        <div className="mb-6">
          <h3 className="text-sm font-semibold text-base-content mb-3">Platforms</h3>
          <div className="text-sm text-base-content/60 italic">
            Coming soon...
          </div>
        </div>

        {/* Rating Filter Section */}
        <div className="mb-6">
          <h3 className="text-sm font-semibold text-base-content mb-3">Rating</h3>
          <div className="text-sm text-base-content/60 italic">
            Coming soon...
          </div>
        </div>

        {/* Release Year Filter Section */}
        <div className="mb-6">
          <h3 className="text-sm font-semibold text-base-content mb-3">Release Year</h3>
          <div className="text-sm text-base-content/60 italic">
            Coming soon...
          </div>
        </div>
      </div>
    </aside>
  );
};

export default Sidebar;