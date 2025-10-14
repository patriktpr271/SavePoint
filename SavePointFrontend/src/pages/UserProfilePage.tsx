import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { UserProfileDto, UserListDto, PagedResult } from '../interfaces/types';
import { User, Calendar, List, Lock, Unlock, Plus, ThumbsUp, ThumbsDown, Globe } from 'lucide-react';

const UserProfilePage: React.FC = () => {
  const { user } = useAuth();
  const [userProfile, setUserProfile] = useState<UserProfileDto | null>(null);
  const [userLists, setUserLists] = useState<UserListDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'profile' | 'lists'>('profile');

  useEffect(() => {
    if (user) {
      fetchUserProfile();
      fetchUserLists();
    }
  }, [user]);

  const fetchUserProfile = async () => {
    try {
      const response = await fetch('/api/auth/me', {
        credentials: 'include',
      });

      if (response.ok) {
        const data = await response.json();
        setUserProfile(data);
      } else {
        setError('Failed to fetch user profile');
      }
    } catch (error) {
      console.error('Error fetching user profile:', error);
      setError('Failed to fetch user profile');
    }
  };

  const fetchUserLists = async () => {
    try {
      const response = await fetch('/api/List/my', {
        credentials: 'include',
      });

      if (response.ok) {
        const data = await response.json();
        setUserLists(data);
      } else {
        setError('Failed to fetch user lists');
      }
    } catch (error) {
      console.error('Error fetching user lists:', error);
      setError('Failed to fetch user lists');
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  };

  const getListTypeIcon = (list: UserListDto) => {
    if (list.defaultListType === 'WantToPlay') {
      return '⭐';
    } else if (list.defaultListType === 'Finished') {
      return '✅';
    }
    return '📋';
  };

  if (!user) {
    return (
      <div className="w-full min-h-screen pt-20 bg-base-200 flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold mb-4">Please log in to view your profile</h1>
          <p className="text-base-content/70">You need to be logged in to access this page.</p>
        </div>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="w-full min-h-screen pt-20 bg-base-200 flex items-center justify-center">
        <div className="loading loading-spinner loading-lg"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="w-full min-h-screen pt-20 bg-base-200 flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold mb-4 text-error">Error</h1>
          <p className="text-base-content/70">{error}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="w-full min-h-screen pt-20 bg-base-200">
      <div className="container mx-auto px-4 py-8">
        {/* Profile Header */}
        <div className="bg-gradient-to-r from-base-100 to-base-200 rounded-xl shadow-xl border border-base-300/50 p-8 mb-8">
          <div className="flex flex-col lg:flex-row items-start lg:items-center justify-between gap-6 mb-8">
            {/* Left side - Profile info */}
            <div className="flex items-center gap-6">
              <div className="relative">
                <div className="w-20 h-20 bg-gradient-to-br from-emerald-500 to-orange-500 rounded-full flex items-center justify-center shadow-lg">
                  <User size={36} className="text-white" />
                </div>
                <div className="absolute -bottom-1 -right-1 w-6 h-6 bg-emerald-500 rounded-full border-2 border-gray-800 flex items-center justify-center">
                  <div className="w-2 h-2 bg-gray-800 rounded-full"></div>
                </div>
              </div>
              <div>
                <h1 className="text-4xl font-bold bg-gradient-to-r from-emerald-500 to-orange-500 bg-clip-text text-transparent">
                  {userProfile?.displayName || user.displayName}
                </h1>
                <p className="text-lg text-base-content/70 font-medium">@{user.username}</p>
                {userProfile?.bio && (
                  <p className="mt-2 text-base-content/80 max-w-md">{userProfile.bio}</p>
                )}
              </div>
            </div>
            
            {/* Right side - Stats */}
            <div className="flex gap-4">
              <div className="bg-gradient-to-br from-emerald-900/30 to-emerald-800/20 rounded-xl p-4 border border-emerald-500/30 min-w-[120px] text-center">
                <div className="flex items-center justify-center mb-2">
                  <List size={24} className="text-emerald-400" />
                </div>
                <div className="text-2xl font-bold text-emerald-300">
                  {userProfile?.listCount || userLists.length}
                </div>
                <div className="text-sm text-base-content/70 font-medium">Total Lists</div>
              </div>
              
              <div className="bg-gradient-to-br from-orange-900/30 to-orange-800/20 rounded-xl p-4 border border-orange-500/30 min-w-[120px] text-center">
                <div className="flex items-center justify-center mb-2">
                  <Unlock size={24} className="text-orange-600" />
                </div>
                <div className="text-2xl font-bold text-orange-700">
                  {userProfile?.publicListCount || userLists.filter(l => l.isPublic).length}
                </div>
                <div className="text-sm text-base-content/70 font-medium">Public Lists</div>
              </div>
            </div>
          </div>

          {/* Tab Navigation */}
          <div className="tabs tabs-boxed mb-6">
            <a 
              className={`tab ${activeTab === 'profile' ? 'tab-active' : ''}`}
              onClick={() => setActiveTab('profile')}
            >
              <User size={16} className="mr-2" />
              Profile Info
            </a>
            <a 
              className={`tab ${activeTab === 'lists' ? 'tab-active' : ''}`}
              onClick={() => setActiveTab('lists')}
            >
              <List size={16} className="mr-2" />
              My Lists ({userLists.length})
            </a>
          </div>

          {/* Tab Content */}
          {activeTab === 'profile' && (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* Account Details Card */}
              <div className="bg-gradient-to-br from-base-200 to-base-300 rounded-xl p-6 border border-base-300/50 shadow-sm">
                <h3 className="text-lg font-semibold mb-4 text-base-content/90">Account Details</h3>
                <div className="space-y-4">
                  <div className="flex items-center gap-3 p-3 bg-base-300 rounded-lg">
                    <User size={18} className="text-emerald-600" />
                    <div>
                      <div className="text-sm text-base-content/60">Username</div>
                      <div className="font-medium">{user.username}</div>
                    </div>
                  </div>
                  <div className="flex items-center gap-3 p-3 bg-base-300 rounded-lg">
                    <div className="w-4 h-4 bg-orange-500 rounded-full"></div>
                    <div>
                      <div className="text-sm text-base-content/60">Email</div>
                      <div className="font-medium">{user.email}</div>
                    </div>
                  </div>
                  {userProfile?.createdAt && (
                    <div className="flex items-center gap-3 p-3 bg-base-300 rounded-lg">
                      <Calendar size={18} className="text-cyan-600" />
                      <div>
                        <div className="text-sm text-base-content/60">Member since</div>
                        <div className="font-medium">{formatDate(userProfile.createdAt)}</div>
                      </div>
                    </div>
                  )}
                </div>
              </div>

              {/* Quick Actions Card */}
              <div className="bg-gradient-to-br from-base-200 to-base-300 rounded-xl p-6 border border-base-300/50 shadow-sm">
                <h3 className="text-lg font-semibold mb-4 text-base-content/90">Quick Actions</h3>
                <div className="space-y-3">
                  <Link 
                    to="/lists" 
                    className="flex items-center gap-3 p-3 bg-emerald-900/20 hover:bg-emerald-800/30 rounded-lg transition-colors group"
                  >
                    <List size={18} className="text-emerald-600" />
                    <span className="font-medium group-hover:text-emerald-700 transition-colors">Manage Lists</span>
                  </Link>
                  <Link 
                    to="/videogames" 
                    className="flex items-center gap-3 p-3 bg-orange-900/20 hover:bg-orange-800/30 rounded-lg transition-colors group"
                  >
                    <div className="w-4 h-4 bg-orange-500 rounded"></div>
                    <span className="font-medium group-hover:text-orange-700 transition-colors">Browse Games</span>
                  </Link>
                  <div className="flex items-center gap-3 p-3 bg-cyan-900/20 rounded-lg transition-colors group cursor-default">
                    <Calendar size={18} className="text-cyan-600" />
                    <div>
                      <div className="text-sm text-base-content/60">Member since</div>
                      <div className="font-medium">{userProfile?.createdAt ? formatDate(userProfile.createdAt) : 'Unknown'}</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          )}

          {activeTab === 'lists' && (
            <div>
              <div className="flex justify-between items-center mb-6">
                <h2 className="text-xl font-semibold">My Lists</h2>
                <Link to="/profile/lists/create" className="btn btn-primary">
                  <Plus size={16} className="mr-2" />
                  Create New List
                </Link>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {userLists.map((list) => (
                  <div key={list.id} className="card bg-base-200 shadow-lg">
                    <div className="card-body">
                      <div className="flex items-start justify-between">
                        <div className="flex items-center gap-2">
                          <span className="text-2xl">{getListTypeIcon(list)}</span>
                          <h3 className="card-title text-lg">{list.name}</h3>
                        </div>
                        <div className="flex items-center gap-1">
                          {list.isPublic ? (
                            <Unlock size={16} className="text-success" />
                          ) : (
                            <Lock size={16} className="text-base-content/60" />
                          )}
                          {list.isDefault && (
                            <span className="badge badge-primary badge-xs">Default</span>
                          )}
                        </div>
                      </div>
                      
                      {list.description && (
                        <p className="text-sm text-base-content/70 mb-3">{list.description}</p>
                      )}

                      <div className="flex items-center justify-between text-sm text-base-content/60">
                        <span>{list.gameCount} games</span>
                        <div className="flex items-center gap-2">
                          <div className="flex items-center gap-1">
                            <ThumbsUp size={14} />
                            <span>{list.upvotes}</span>
                          </div>
                          <div className="flex items-center gap-1">
                            <ThumbsDown size={14} />
                            <span>{list.downvotes}</span>
                          </div>
                        </div>
                      </div>

                      <div className="card-actions justify-end mt-4">
                        <Link to={`/lists/${list.id}`} className="btn btn-sm btn-outline">
                          View List
                        </Link>
                        {!list.isDefault && (
                          <Link to={`/lists/${list.id}/edit`} className="btn btn-sm btn-primary">
                            Edit
                          </Link>
                        )}
                      </div>
                    </div>
                  </div>
                ))}
              </div>

              {userLists.length === 0 && (
                <div className="text-center py-12">
                  <List size={64} className="mx-auto text-base-content/30 mb-4" />
                  <h3 className="text-xl font-semibold mb-2">No lists yet</h3>
                  <p className="text-base-content/70 mb-4">Create your first list to start organizing your games!</p>
                  <Link to="/profile/lists/create" className="btn btn-primary">
                    <Plus size={16} className="mr-2" />
                    Create Your First List
                  </Link>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default UserProfilePage;