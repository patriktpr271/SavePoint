import React, { useState, useEffect } from 'react';
import { Link, useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { UserListDto, PagedResult, GameCardDto, CreateUserListDto, UpdateUserListDto, VoteOnListDto } from '../interfaces/types';
import { 
  Search, 
  Filter, 
  Plus, 
  Edit, 
  Trash2, 
  Lock, 
  Unlock, 
  ThumbsUp, 
  ThumbsDown, 
  Eye,
  Users,
  Calendar,
  Star,
  CheckCircle,
  List as ListIcon
} from 'lucide-react';

const UserListsPage: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [lists, setLists] = useState<UserListDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [sortBy, setSortBy] = useState('name');
  const [showOnlyPublic, setShowOnlyPublic] = useState(false);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(12);
  const [totalPages, setTotalPages] = useState(0);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [editingList, setEditingList] = useState<UserListDto | null>(null);
  const [newList, setNewList] = useState<CreateUserListDto>({
    name: '',
    description: '',
    isPublic: false
  });

  useEffect(() => {
    if (showOnlyPublic) {
      fetchPublicLists();
    } else if (user) {
      fetchMyLists();
    }
  }, [user, searchTerm, sortBy, pageNumber, showOnlyPublic]);

  const fetchMyLists = async () => {
    try {
      setLoading(true);
      const response = await fetch('/api/List/my', {
        credentials: 'include',
      });

      if (response.ok) {
        const data = await response.json();
        setLists(data);
        setTotalPages(1);
      } else {
        setError('Failed to fetch your lists');
      }
    } catch (error) {
      console.error('Error fetching user lists:', error);
      setError('Failed to fetch your lists');
    } finally {
      setLoading(false);
    }
  };

  const fetchPublicLists = async () => {
    try {
      setLoading(true);
      const params = new URLSearchParams({
        pageNumber: pageNumber.toString(),
        pageSize: pageSize.toString(),
        ...(searchTerm && { search: searchTerm }),
        ...(sortBy && { sortBy })
      });

      const response = await fetch(`/api/List/public?${params}`, {
        credentials: 'include',
      });

      if (response.ok) {
        const data: PagedResult<UserListDto> = await response.json();
        setLists(data.items);
        setTotalPages(data.totalPages);
      } else {
        setError('Failed to fetch public lists');
      }
    } catch (error) {
      console.error('Error fetching public lists:', error);
      setError('Failed to fetch public lists');
    } finally {
      setLoading(false);
    }
  };

  const createList = async () => {
    try {
      const response = await fetch('/api/List', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify(newList),
      });

      if (response.ok) {
        setShowCreateModal(false);
        setNewList({ name: '', description: '', isPublic: false });
        if (!showOnlyPublic) {
          fetchMyLists();
        }
      } else {
        setError('Failed to create list');
      }
    } catch (error) {
      console.error('Error creating list:', error);
      setError('Failed to create list');
    }
  };

  const updateList = async (listId: string, updateData: UpdateUserListDto) => {
    try {
      const response = await fetch(`/api/List/${listId}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify(updateData),
      });

      if (response.ok) {
        setEditingList(null);
        if (showOnlyPublic) {
          fetchPublicLists();
        } else {
          fetchMyLists();
        }
      } else {
        setError('Failed to update list');
      }
    } catch (error) {
      console.error('Error updating list:', error);
      setError('Failed to update list');
    }
  };

  const deleteList = async (listId: string) => {
    if (!confirm('Are you sure you want to delete this list? This action cannot be undone.')) {
      return;
    }

    try {
      const response = await fetch(`/api/List/${listId}`, {
        method: 'DELETE',
        credentials: 'include',
      });

      if (response.ok) {
        if (showOnlyPublic) {
          fetchPublicLists();
        } else {
          fetchMyLists();
        }
      } else {
        setError('Failed to delete list');
      }
    } catch (error) {
      console.error('Error deleting list:', error);
      setError('Failed to delete list');
    }
  };

  const voteOnList = async (listId: string, isUpvote: boolean) => {
    try {
      const voteData: VoteOnListDto = { isUpvote };
      const response = await fetch(`/api/List/${listId}/vote`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify(voteData),
      });

      if (response.ok) {
        if (showOnlyPublic) {
          fetchPublicLists();
        } else {
          fetchMyLists();
        }
      } else {
        setError('Failed to vote on list');
      }
    } catch (error) {
      console.error('Error voting on list:', error);
      setError('Failed to vote on list');
    }
  };

  const removeVote = async (listId: string) => {
    try {
      const response = await fetch(`/api/List/${listId}/vote`, {
        method: 'DELETE',
        credentials: 'include',
      });

      if (response.ok) {
        if (showOnlyPublic) {
          fetchPublicLists();
        } else {
          fetchMyLists();
        }
      } else {
        setError('Failed to remove vote');
      }
    } catch (error) {
      console.error('Error removing vote:', error);
      setError('Failed to remove vote');
    }
  };

  const getDefaultList = async (defaultListType: string) => {
    try {
      const response = await fetch(`/api/List/default/${defaultListType}`, {
        credentials: 'include',
      });

      if (response.ok) {
        const data = await response.json();
        return data;
      }
    } catch (error) {
      console.error('Error fetching default list:', error);
    }
    return null;
  };

  const addToWantToPlay = async (gameId: string) => {
    try {
      const response = await fetch(`/api/List/want-to-play/${gameId}`, {
        method: 'POST',
        credentials: 'include',
      });

      if (response.ok) {
        if (showOnlyPublic) {
          fetchMyLists();
        }
      } else {
        setError('Failed to add game to Want to Play list');
      }
    } catch (error) {
      console.error('Error adding to want to play:', error);
      setError('Failed to add game to Want to Play list');
    }
  };

  const addToFinished = async (gameId: string) => {
    try {
      const response = await fetch(`/api/List/finished/${gameId}`, {
        method: 'POST',
        credentials: 'include',
      });

      if (response.ok) {
        if (!showOnlyPublic) {
          fetchMyLists();
        }
      } else {
        setError('Failed to add game to Finished list');
      }
    } catch (error) {
      console.error('Error adding to finished:', error);
      setError('Failed to add game to Finished list');
    }
  };

  const moveToFinished = async (gameId: string) => {
    try {
      const response = await fetch(`/api/List/move-to-finished/${gameId}`, {
        method: 'POST',
        credentials: 'include',
      });

      if (response.ok) {
        if (!showOnlyPublic) {
          fetchMyLists();
        }
      } else {
        setError('Failed to move game to Finished list');
      }
    } catch (error) {
      console.error('Error moving to finished:', error);
      setError('Failed to move game to Finished list');
    }
  };

  const getListTypeIcon = (list: UserListDto) => {
    if (list.defaultListType === 'WantToPlay') {
      return <Star className="text-yellow-500" size={20} />;
    } else if (list.defaultListType === 'Finished') {
      return <CheckCircle className="text-green-500" size={20} />;
    }
    return <ListIcon className="text-blue-500" size={20} />;
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  const filteredLists = lists.filter(list =>
    list.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    (list.description?.toLowerCase().includes(searchTerm.toLowerCase()) ?? false)
  );

  if (!user && !showOnlyPublic) {
    return (
      <div className="w-full min-h-screen pt-20 bg-base-200 flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold mb-4">Please log in to view your lists</h1>
          <p className="text-base-content/70 mb-4">You need to be logged in to manage your lists.</p>
          <div className="space-x-4">
            <button 
              className="btn btn-primary"
              onClick={() => setShowOnlyPublic(true)}
            >
              Browse Public Lists
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="w-full min-h-screen pt-20 bg-base-200">
      <div className="container mx-auto px-4 py-8">
        {/* Header */}
        <div className="flex flex-col md:flex-row justify-between items-start md:items-center mb-8">
          <div>
            <h1 className="text-3xl font-bold mb-2">
              {showOnlyPublic ? 'Public Lists' : 'My Lists'}
            </h1>
            <p className="text-base-content/70">
              {showOnlyPublic 
                ? 'Discover and vote on community game lists'
                : 'Organize your games into personalized lists'
              }
            </p>
          </div>

          <div className="flex gap-2 mt-4 md:mt-0">
            <div className="tabs tabs-boxed">
              <a 
                className={`tab ${!showOnlyPublic ? 'tab-active' : ''}`}
                onClick={() => setShowOnlyPublic(false)}
              >
                My Lists
              </a>
              <a 
                className={`tab ${showOnlyPublic ? 'tab-active' : ''}`}
                onClick={() => setShowOnlyPublic(true)}
              >
                Public Lists
              </a>
            </div>
            
            {!showOnlyPublic && user && (
              <button 
                className="btn btn-primary"
                onClick={() => setShowCreateModal(true)}
              >
                <Plus size={16} className="mr-2" />
                Create List
              </button>
            )}
          </div>
        </div>

        {/* Filters */}
        <div className="bg-base-100 rounded-lg shadow-lg p-4 mb-6">
          <div className="flex flex-col md:flex-row gap-4">
            <div className="flex-1">
              <div className="input-group">
                <span>
                  <Search size={20} />
                </span>
                <input 
                  type="text" 
                  placeholder="Search lists..." 
                  className="input input-bordered w-full"
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                />
              </div>
            </div>
            
            <select 
              className="select select-bordered"
              value={sortBy}
              onChange={(e) => setSortBy(e.target.value)}
            >
              <option value="name">Sort by Name</option>
              <option value="createdAt">Sort by Date Created</option>
              <option value="updatedAt">Sort by Last Updated</option>
              <option value="gameCount">Sort by Game Count</option>
              {showOnlyPublic && <option value="upvotes">Sort by Popularity</option>}
            </select>
          </div>
        </div>

        {/* Loading State */}
        {loading && (
          <div className="flex justify-center items-center py-12">
            <div className="loading loading-spinner loading-lg"></div>
          </div>
        )}

        {/* Error State */}
        {error && (
          <div className="alert alert-error mb-6">
            <span>{error}</span>
            <button 
              className="btn btn-sm btn-ghost"
              onClick={() => setError(null)}
            >
              Dismiss
            </button>
          </div>
        )}

        {/* Lists Grid */}
        {!loading && (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
            {filteredLists.map((list) => (
              <div key={list.id} className="card bg-base-100 shadow-lg hover:shadow-xl transition-shadow">
                <div className="card-body">
                  <div className="flex items-start justify-between mb-2">
                    <div className="flex items-center gap-2">
                      {getListTypeIcon(list)}
                      <h3 className="card-title text-lg truncate">{list.name}</h3>
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
                    <p className="text-sm text-base-content/70 mb-3 line-clamp-2">{list.description}</p>
                  )}

                  <div className="flex items-center justify-between text-sm text-base-content/60 mb-3">
                    <span className="flex items-center gap-1">
                      <Eye size={14} />
                      {list.gameCount} games
                    </span>
                    {list.userName && showOnlyPublic && (
                      <span className="flex items-center gap-1">
                        <Users size={14} />
                        {list.userName}
                      </span>
                    )}
                  </div>

                  <div className="flex items-center justify-between text-sm text-base-content/60 mb-4">
                    <span className="flex items-center gap-1">
                      <Calendar size={14} />
                      {formatDate(list.createdAt)}
                    </span>
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

                  <div className="card-actions justify-between">
                    <div className="flex gap-1">
                      {showOnlyPublic && user && (
                        <>
                          <button
                            className={`btn btn-xs ${list.userVote === true ? 'btn-success' : 'btn-outline'}`}
                            onClick={() => list.userVote === true ? removeVote(list.id) : voteOnList(list.id, true)}
                          >
                            <ThumbsUp size={12} />
                          </button>
                          <button
                            className={`btn btn-xs ${list.userVote === false ? 'btn-error' : 'btn-outline'}`}
                            onClick={() => list.userVote === false ? removeVote(list.id) : voteOnList(list.id, false)}
                          >
                            <ThumbsDown size={12} />
                          </button>
                        </>
                      )}
                    </div>

                    <div className="flex gap-1">
                      <Link to={`/lists/${list.id}`} className="btn btn-xs btn-outline">
                        <Eye size={12} />
                        View
                      </Link>
                      {!showOnlyPublic && user && list.userId === user.id && !list.isDefault && (
                        <>
                          <button
                            className="btn btn-xs btn-primary"
                            onClick={() => setEditingList(list)}
                          >
                            <Edit size={12} />
                          </button>
                          <button
                            className="btn btn-xs btn-error"
                            onClick={() => deleteList(list.id)}
                          >
                            <Trash2 size={12} />
                          </button>
                        </>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Empty State */}
        {!loading && filteredLists.length === 0 && (
          <div className="text-center py-12">
            <ListIcon size={64} className="mx-auto text-base-content/30 mb-4" />
            <h3 className="text-xl font-semibold mb-2">
              {showOnlyPublic ? 'No public lists found' : 'No lists yet'}
            </h3>
            <p className="text-base-content/70 mb-4">
              {showOnlyPublic 
                ? 'Try adjusting your search or check back later for new public lists.'
                : 'Create your first list to start organizing your games!'
              }
            </p>
            {!showOnlyPublic && user && (
              <button 
                className="btn btn-primary"
                onClick={() => setShowCreateModal(true)}
              >
                <Plus size={16} className="mr-2" />
                Create Your First List
              </button>
            )}
          </div>
        )}

        {/* Pagination */}
        {showOnlyPublic && totalPages > 1 && (
          <div className="flex justify-center mt-8">
            <div className="join">
              <button
                className="join-item btn"
                disabled={pageNumber <= 1}
                onClick={() => setPageNumber(pageNumber - 1)}
              >
                «
              </button>
              <span className="join-item btn btn-disabled">
                Page {pageNumber} of {totalPages}
              </span>
              <button
                className="join-item btn"
                disabled={pageNumber >= totalPages}
                onClick={() => setPageNumber(pageNumber + 1)}
              >
                »
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Create List Modal */}
      {showCreateModal && (
        <div className="modal modal-open">
          <div className="modal-box">
            <h3 className="font-bold text-lg mb-4">Create New List</h3>
            <div className="space-y-4">
              <div>
                <label className="label">
                  <span className="label-text">List Name</span>
                </label>
                <input
                  type="text"
                  className="input input-bordered w-full"
                  value={newList.name}
                  onChange={(e) => setNewList({ ...newList, name: e.target.value })}
                  placeholder="Enter list name..."
                />
              </div>
              <div>
                <label className="label">
                  <span className="label-text">Description (optional)</span>
                </label>
                <textarea
                  className="textarea textarea-bordered w-full"
                  value={newList.description}
                  onChange={(e) => setNewList({ ...newList, description: e.target.value })}
                  placeholder="Describe your list..."
                />
              </div>
              <div className="form-control">
                <label className="label cursor-pointer">
                  <span className="label-text">Make list public</span>
                  <input
                    type="checkbox"
                    className="checkbox"
                    checked={newList.isPublic}
                    onChange={(e) => setNewList({ ...newList, isPublic: e.target.checked })}
                  />
                </label>
              </div>
            </div>
            <div className="modal-action">
              <button 
                className="btn btn-ghost" 
                onClick={() => {
                  setShowCreateModal(false);
                  setNewList({ name: '', description: '', isPublic: false });
                }}
              >
                Cancel
              </button>
              <button 
                className="btn btn-primary"
                onClick={createList}
                disabled={!newList.name.trim()}
              >
                Create List
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Edit List Modal */}
      {editingList && (
        <div className="modal modal-open">
          <div className="modal-box">
            <h3 className="font-bold text-lg mb-4">Edit List</h3>
            <div className="space-y-4">
              <div>
                <label className="label">
                  <span className="label-text">List Name</span>
                </label>
                <input
                  type="text"
                  className="input input-bordered w-full"
                  value={editingList.name}
                  onChange={(e) => setEditingList({ ...editingList, name: e.target.value })}
                />
              </div>
              <div>
                <label className="label">
                  <span className="label-text">Description (optional)</span>
                </label>
                <textarea
                  className="textarea textarea-bordered w-full"
                  value={editingList.description || ''}
                  onChange={(e) => setEditingList({ ...editingList, description: e.target.value })}
                />
              </div>
              <div className="form-control">
                <label className="label cursor-pointer">
                  <span className="label-text">Make list public</span>
                  <input
                    type="checkbox"
                    className="checkbox"
                    checked={editingList.isPublic}
                    onChange={(e) => setEditingList({ ...editingList, isPublic: e.target.checked })}
                  />
                </label>
              </div>
            </div>
            <div className="modal-action">
              <button 
                className="btn btn-ghost" 
                onClick={() => setEditingList(null)}
              >
                Cancel
              </button>
              <button 
                className="btn btn-primary"
                onClick={() => updateList(editingList.id, {
                  name: editingList.name,
                  description: editingList.description || '',
                  isPublic: editingList.isPublic
                })}
                disabled={!editingList.name.trim()}
              >
                Save Changes
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default UserListsPage;