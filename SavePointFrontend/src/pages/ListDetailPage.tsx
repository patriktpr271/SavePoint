import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { UserListDto, GameCardDto, AddGameToListDto, VoteOnListDto } from '../interfaces/types';
import GameCard from '../components/GameCard';
import { 
  ArrowLeft,
  Edit,
  Trash2,
  Lock,
  Unlock,
  ThumbsUp,
  ThumbsDown,
  Plus,
  Search,
  Star,
  CheckCircle,
  List as ListIcon,
  Users,
  Calendar,
  Eye
} from 'lucide-react';

const ListDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();
  const navigate = useNavigate();
  const [list, setList] = useState<UserListDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showAddGameModal, setShowAddGameModal] = useState(false);
  const [gameSearchQuery, setGameSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState<GameCardDto[]>([]);
  const [searchLoading, setSearchLoading] = useState(false);

  useEffect(() => {
    if (id) {
      fetchListDetail();
    }
  }, [id]);

  const fetchListDetail = async () => {
    try {
      setLoading(true);
      const response = await fetch(`/api/List/${id}?includeGames=true`, {
        credentials: 'include',
      });

      if (response.ok) {
        const data = await response.json();
        setList(data);
      } else if (response.status === 404) {
        setError('List not found');
      } else if (response.status === 403) {
        setError('You do not have permission to view this list');
      } else {
        setError('Failed to fetch list');
      }
    } catch (error) {
      console.error('Error fetching list:', error);
      setError('Failed to fetch list');
    } finally {
      setLoading(false);
    }
  };

  const searchGames = async (query: string) => {
    if (!query.trim()) {
      setSearchResults([]);
      return;
    }

    try {
      setSearchLoading(true);
      const response = await fetch(`/api/Game?search=${encodeURIComponent(query)}&pageSize=10`, {
        credentials: 'include',
      });

      if (response.ok) {
        const data = await response.json();
        setSearchResults(data.items || []);
      }
    } catch (error) {
      console.error('Error searching games:', error);
    } finally {
      setSearchLoading(false);
    }
  };

  const addGameToList = async (gameId: string) => {
    if (!list || !user) return;

    try {
      const addGameData: AddGameToListDto = { gameId };
      const response = await fetch(`/api/List/${list.id}/games`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify(addGameData),
      });

      if (response.ok) {
        setShowAddGameModal(false);
        setGameSearchQuery('');
        setSearchResults([]);
        fetchListDetail();
      } else {
        setError('Failed to add game to list');
      }
    } catch (error) {
      console.error('Error adding game to list:', error);
      setError('Failed to add game to list');
    }
  };

  const removeGameFromList = async (gameId: string) => {
    if (!list || !user) return;

    if (!confirm('Remove this game from the list?')) return;

    try {
      const response = await fetch(`/api/List/${list.id}/games/${gameId}`, {
        method: 'DELETE',
        credentials: 'include',
      });

      if (response.ok) {
        fetchListDetail();
      } else {
        setError('Failed to remove game from list');
      }
    } catch (error) {
      console.error('Error removing game from list:', error);
      setError('Failed to remove game from list');
    }
  };

  const voteOnList = async (isUpvote: boolean) => {
    if (!list || !user) return;

    try {
      const voteData: VoteOnListDto = { isUpvote };
      const response = await fetch(`/api/List/${list.id}/vote`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify(voteData),
      });

      if (response.ok) {
        fetchListDetail();
      } else {
        setError('Failed to vote on list');
      }
    } catch (error) {
      console.error('Error voting on list:', error);
      setError('Failed to vote on list');
    }
  };

  const removeVote = async () => {
    if (!list || !user) return;

    try {
      const response = await fetch(`/api/List/${list.id}/vote`, {
        method: 'DELETE',
        credentials: 'include',
      });

      if (response.ok) {
        fetchListDetail();
      } else {
        setError('Failed to remove vote');
      }
    } catch (error) {
      console.error('Error removing vote:', error);
      setError('Failed to remove vote');
    }
  };

  const deleteList = async () => {
    if (!list || !user) return;

    if (!confirm('Are you sure you want to delete this list? This action cannot be undone.')) {
      return;
    }

    try {
      const response = await fetch(`/api/List/${list.id}`, {
        method: 'DELETE',
        credentials: 'include',
      });

      if (response.ok) {
        navigate('/lists');
      } else {
        setError('Failed to delete list');
      }
    } catch (error) {
      console.error('Error deleting list:', error);
      setError('Failed to delete list');
    }
  };

  const getListTypeIcon = (list: UserListDto) => {
    if (list.defaultListType === 'WantToPlay') {
      return <Star className="text-yellow-500" size={24} />;
    } else if (list.defaultListType === 'Finished') {
      return <CheckCircle className="text-green-500" size={24} />;
    }
    return <ListIcon className="text-blue-500" size={24} />;
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  };

  const isOwner = user && list && list.userId === user.id;
  const canEdit = isOwner && !list?.isDefault;

  if (loading) {
    return (
      <div className="w-full min-h-screen pt-20 bg-base-200 flex items-center justify-center">
        <div className="loading loading-spinner loading-lg"></div>
      </div>
    );
  }

  if (error || !list) {
    return (
      <div className="w-full min-h-screen pt-20 bg-base-200 flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold mb-4 text-error">Error</h1>
          <p className="text-base-content/70 mb-4">{error || 'List not found'}</p>
          <Link to="/lists" className="btn btn-primary">
            <ArrowLeft size={16} className="mr-2" />
            Back to Lists
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="w-full min-h-screen pt-20 bg-base-200">
      <div className="container mx-auto px-4 py-8">
        {/* Header */}
        <div className="flex items-center gap-4 mb-6">
          <Link to="/lists" className="btn btn-ghost btn-circle">
            <ArrowLeft size={20} />
          </Link>
          <div className="flex items-center gap-3">
            {getListTypeIcon(list)}
            <h1 className="text-3xl font-bold">{list.name}</h1>
          </div>
        </div>

        {/* List Info Card */}
        <div className="bg-base-100 rounded-lg shadow-lg p-6 mb-8">
          <div className="flex flex-col md:flex-row justify-between items-start gap-6">
            <div className="flex-1">
              <div className="flex items-center gap-2 mb-3">
                {list.isPublic ? (
                  <Unlock size={20} className="text-success" />
                ) : (
                  <Lock size={20} className="text-base-content/60" />
                )}
                <span className="text-sm text-base-content/70">
                  {list.isPublic ? 'Public List' : 'Private List'}
                </span>
                {list.isDefault && (
                  <span className="badge badge-primary">Default List</span>
                )}
              </div>

              {list.description && (
                <p className="text-base-content/80 mb-4">{list.description}</p>
              )}

              <div className="flex flex-wrap gap-4 text-sm text-base-content/60">
                {list.userName && (
                  <div className="flex items-center gap-1">
                    <Users size={16} />
                    <span>By {list.userName}</span>
                  </div>
                )}
                <div className="flex items-center gap-1">
                  <Eye size={16} />
                  <span>{list.gameCount} games</span>
                </div>
                <div className="flex items-center gap-1">
                  <Calendar size={16} />
                  <span>Created {formatDate(list.createdAt)}</span>
                </div>
              </div>
            </div>

            <div className="flex flex-col items-end gap-4">
              {/* Voting */}
              {user && !isOwner && (
                <div className="flex items-center gap-2">
                  <button
                    className={`btn btn-sm ${list.userVote === true ? 'btn-success' : 'btn-outline'}`}
                    onClick={() => list.userVote === true ? removeVote() : voteOnList(true)}
                  >
                    <ThumbsUp size={16} />
                    {list.upvotes}
                  </button>
                  <button
                    className={`btn btn-sm ${list.userVote === false ? 'btn-error' : 'btn-outline'}`}
                    onClick={() => list.userVote === false ? removeVote() : voteOnList(false)}
                  >
                    <ThumbsDown size={16} />
                    {list.downvotes}
                  </button>
                </div>
              )}

              {/* Owner Actions */}
              {isOwner && (
                <div className="flex gap-2">
                  {canEdit && (
                    <Link to={`/lists/${list.id}/edit`} className="btn btn-sm btn-primary">
                      <Edit size={16} />
                    </Link>
                  )}
                  <button
                    className="btn btn-sm btn-success"
                    onClick={() => setShowAddGameModal(true)}
                  >
                    <Plus size={16} />
                    Add Game
                  </button>
                  {canEdit && (
                    <button
                      className="btn btn-sm btn-error"
                      onClick={deleteList}
                    >
                      <Trash2 size={16} />
                    </button>
                  )}
                </div>
              )}

              {/* Vote counts for non-owners */}
              {!isOwner && (
                <div className="flex items-center gap-4 text-sm text-base-content/60">
                  <div className="flex items-center gap-1">
                    <ThumbsUp size={16} />
                    <span>{list.upvotes}</span>
                  </div>
                  <div className="flex items-center gap-1">
                    <ThumbsDown size={16} />
                    <span>{list.downvotes}</span>
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Games Grid */}
        <div>
          <h2 className="text-2xl font-semibold mb-6">
            Games ({list.games?.length || 0})
          </h2>

          {list.games && list.games.length > 0 ? (
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 2xl:grid-cols-6 gap-6">
              {list.games.map((game) => (
                <div key={game.id} className="relative">
                  <GameCard game={game} />
                  {isOwner && (
                    <button
                      className="absolute top-2 right-2 btn btn-xs btn-circle btn-error opacity-80 hover:opacity-100"
                      onClick={() => removeGameFromList(game.id)}
                    >
                      <Trash2 size={12} />
                    </button>
                  )}
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-12">
              <ListIcon size={64} className="mx-auto text-base-content/30 mb-4" />
              <h3 className="text-xl font-semibold mb-2">No games in this list yet</h3>
              <p className="text-base-content/70 mb-4">
                {isOwner 
                  ? 'Start adding games to build your collection!' 
                  : 'This list is empty.'
                }
              </p>
              {isOwner && (
                <button 
                  className="btn btn-primary"
                  onClick={() => setShowAddGameModal(true)}
                >
                  <Plus size={16} className="mr-2" />
                  Add Your First Game
                </button>
              )}
            </div>
          )}
        </div>
      </div>

      {/* Add Game Modal */}
      {showAddGameModal && (
        <div className="modal modal-open">
          <div className="modal-box max-w-2xl">
            <h3 className="font-bold text-lg mb-4">Add Game to List</h3>
            
            <div className="mb-4">
              <div className="input-group">
                <span>
                  <Search size={20} />
                </span>
                <input 
                  type="text" 
                  placeholder="Search for games..." 
                  className="input input-bordered w-full"
                  value={gameSearchQuery}
                  onChange={(e) => {
                    setGameSearchQuery(e.target.value);
                    searchGames(e.target.value);
                  }}
                />
              </div>
            </div>

            <div className="max-h-96 overflow-y-auto">
              {searchLoading && (
                <div className="flex justify-center py-4">
                  <div className="loading loading-spinner loading-sm"></div>
                </div>
              )}

              {searchResults.length > 0 && (
                <div className="space-y-2">
                  {searchResults.map((game) => (
                    <div key={game.id} className="flex items-center gap-3 p-3 bg-base-200 rounded-lg">
                      {game.coverUrl && (
                        <img 
                          src={game.coverUrl} 
                          alt={game.name} 
                          className="w-12 h-16 object-cover rounded"
                        />
                      )}
                      <div className="flex-1">
                        <h4 className="font-semibold">{game.name}</h4>
                        <p className="text-sm text-base-content/60">
                          {new Date(game.releaseDate).getFullYear()}
                        </p>
                      </div>
                      <button
                        className="btn btn-sm btn-primary"
                        onClick={() => addGameToList(game.id)}
                      >
                        Add
                      </button>
                    </div>
                  ))}
                </div>
              )}

              {gameSearchQuery && !searchLoading && searchResults.length === 0 && (
                <div className="text-center py-8 text-base-content/60">
                  No games found for "{gameSearchQuery}"
                </div>
              )}

              {!gameSearchQuery && (
                <div className="text-center py-8 text-base-content/60">
                  Start typing to search for games
                </div>
              )}
            </div>

            <div className="modal-action">
              <button 
                className="btn btn-ghost" 
                onClick={() => {
                  setShowAddGameModal(false);
                  setGameSearchQuery('');
                  setSearchResults([]);
                }}
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ListDetailPage;