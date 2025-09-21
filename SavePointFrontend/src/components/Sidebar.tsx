import React from 'react';

interface SidebarProps {
  className?: string;
}

const Sidebar: React.FC<SidebarProps> = ({ className = "" }) => {
  return (
    <aside className={`hidden lg:block w-64 xl:w-80 bg-base-200/30 border-r border-base-300 ${className}`}>
      <div className="p-6">
        <div className="text-lg font-semibold text-base-content/60 mb-4">
          Filters & Categories
        </div>
        
        {/* Genre Filter Section */}
        <div className="mb-6">
          <h3 className="text-sm font-medium text-base-content/80 mb-2">Genres</h3>
          <div className="text-sm text-base-content/40">
            Coming soon...
          </div>
        </div>

        {/* Platform Filter Section */}
        <div className="mb-6">
          <h3 className="text-sm font-medium text-base-content/80 mb-2">Platforms</h3>
          <div className="text-sm text-base-content/40">
            Coming soon...
          </div>
        </div>

        {/* Rating Filter Section */}
        <div className="mb-6">
          <h3 className="text-sm font-medium text-base-content/80 mb-2">Rating</h3>
          <div className="text-sm text-base-content/40">
            Coming soon...
          </div>
        </div>

        {/* Release Year Filter Section */}
        <div className="mb-6">
          <h3 className="text-sm font-medium text-base-content/80 mb-2">Release Year</h3>
          <div className="text-sm text-base-content/40">
            Coming soon...
          </div>
        </div>
      </div>
    </aside>
  );
};

export default Sidebar;