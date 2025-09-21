import React, { useState, useEffect } from 'react';
import GameCard from './GameCard';
import Sidebar from './Sidebar';

interface Game {
  id: number;
  name: string;
  summary?: string;
  coverUrl?: string;
  rating: number;
  releaseDate: string;
  gameGenres?: Array<{ id: number; name: string }>;
  gamePlatforms?: Array<{ id: number; name: string }>;
  gameCompanies?: Array<{ id: number; name: string; role: string }>;
}

const HomeScreen: React.FC = () => {
  const [games, setGames] = useState<Game[]>([]);
  const [loading, setLoading] = useState(true);

  // Sample data - replace with actual API call
  const sampleGames: Game[] = [
    {
      id: 1,
      name: "The Witcher 3: Wild Hunt",
      summary: "The Witcher 3: Wild Hunt is a story-driven, next-generation open world role-playing game set in a visually stunning fantasy universe full of meaningful choices and impactful consequences.",
      coverUrl: "https://images.igdb.com/igdb/image/upload/t_cover_big/co1r7n.webp",
      rating: 9.3,
      releaseDate: "2015-05-19T00:00:00Z",
      gameGenres: [
        { id: 1, name: "RPG" },
        { id: 2, name: "Adventure" },
        { id: 3, name: "Open World" }
      ],
      gamePlatforms: [
        { id: 1, name: "PC" },
        { id: 2, name: "PlayStation 4" },
        { id: 3, name: "Xbox One" },
        { id: 4, name: "Nintendo Switch" }
      ],
      gameCompanies: [
        { id: 1, name: "CD Projekt RED", role: "Developer" },
        { id: 2, name: "CD Projekt", role: "Publisher" }
      ]
    },
    {
      id: 2,
      name: "Cyberpunk 2077",
      summary: "Cyberpunk 2077 is an open-world, action-adventure story set in Night City, a megalopolis obsessed with power, glamour and body modification.",
      coverUrl: "https://images.igdb.com/igdb/image/upload/t_cover_big/co2lbd.webp",
      rating: 7.2,
      releaseDate: "2020-12-10T00:00:00Z",
      gameGenres: [
        { id: 4, name: "Action" },
        { id: 5, name: "RPG" },
        { id: 6, name: "Sci-Fi" }
      ],
      gamePlatforms: [
        { id: 1, name: "PC" },
        { id: 2, name: "PlayStation 4" },
        { id: 3, name: "Xbox One" }
      ],
      gameCompanies: [
        { id: 1, name: "CD Projekt RED", role: "Developer" },
        { id: 2, name: "CD Projekt", role: "Publisher" }
      ]
    },
    {
      id: 3,
      name: "Red Dead Redemption 2",
      summary: "America, 1899. The end of the Wild West era has begun. After a robbery goes badly wrong in the western town of Blackwater, Arthur Morgan and the Van der Linde gang are forced to flee.",
      coverUrl: "https://images.igdb.com/igdb/image/upload/t_cover_big/co1q1f.webp",
      rating: 9.7,
      releaseDate: "2018-10-26T00:00:00Z",
      gameGenres: [
        { id: 7, name: "Action" },
        { id: 8, name: "Adventure" },
        { id: 9, name: "Western" }
      ],
      gamePlatforms: [
        { id: 1, name: "PC" },
        { id: 2, name: "PlayStation 4" },
        { id: 3, name: "Xbox One" }
      ],
      gameCompanies: [
        { id: 3, name: "Rockstar Games", role: "Developer" },
        { id: 4, name: "Rockstar Games", role: "Publisher" }
      ]
    },
    {
      id: 4,
      name: "God of War",
      summary: "It is a new beginning for Kratos. Living as a man, outside the shadow of the gods, he seeks solitude in the unfamiliar lands of Norse mythology.",
      coverUrl: "https://images.igdb.com/igdb/image/upload/t_cover_big/co1tmu.webp",
      rating: 9.4,
      releaseDate: "2018-04-20T00:00:00Z",
      gameGenres: [
        { id: 10, name: "Action" },
        { id: 11, name: "Adventure" },
        { id: 12, name: "Mythology" }
      ],
      gamePlatforms: [
        { id: 2, name: "PlayStation 4" },
        { id: 1, name: "PC" }
      ],
      gameCompanies: [
        { id: 5, name: "Santa Monica Studio", role: "Developer" },
        { id: 6, name: "Sony Interactive Entertainment", role: "Publisher" }
      ]
    },
    {
      id: 5,
      name: "Hades",
      summary: "Hades is a god-like rogue-like dungeon crawler that combines the best aspects of Supergiant's critically acclaimed titles.",
      coverUrl: "https://images.igdb.com/igdb/image/upload/t_cover_big/co2i9s.webp",
      rating: 9.0,
      releaseDate: "2020-09-17T00:00:00Z",
      gameGenres: [
        { id: 13, name: "Roguelike" },
        { id: 14, name: "Action" },
        { id: 15, name: "Indie" }
      ],
      gamePlatforms: [
        { id: 1, name: "PC" },
        { id: 4, name: "Nintendo Switch" },
        { id: 2, name: "PlayStation 4" },
        { id: 3, name: "Xbox One" }
      ],
      gameCompanies: [
        { id: 7, name: "Supergiant Games", role: "Developer" },
        { id: 7, name: "Supergiant Games", role: "Publisher" }
      ]
    },
    {
      id: 6,
      name: "Elden Ring",
      summary: "THE NEW FANTASY ACTION RPG. Rise, Tarnished, and be guided by grace to brandish the power of the Elden Ring and become an Elden Lord in the Lands Between.",
      coverUrl: "https://images.igdb.com/igdb/image/upload/t_cover_big/co4jni.webp",
      rating: 9.2,
      releaseDate: "2022-02-25T00:00:00Z",
      gameGenres: [
        { id: 16, name: "Action RPG" },
        { id: 17, name: "Dark Fantasy" },
        { id: 18, name: "Open World" }
      ],
      gamePlatforms: [
        { id: 1, name: "PC" },
        { id: 2, name: "PlayStation 4" },
        { id: 19, name: "PlayStation 5" },
        { id: 3, name: "Xbox One" },
        { id: 20, name: "Xbox Series X/S" }
      ],
      gameCompanies: [
        { id: 8, name: "FromSoftware", role: "Developer" },
        { id: 9, name: "Bandai Namco Entertainment", role: "Publisher" }
      ]
    }
  ];

  useEffect(() => {
    // Simulate API call 
    const fetchGames = async () => {
      setLoading(true);
      
      // Delay
      setTimeout(() => {
        setGames(sampleGames);
        setLoading(false);
      }, 1000);
    };

    fetchGames();
  }, []);

  if (loading) {
    return (
      <div className="min-h-screen pt-20 bg-base-100">
        <div className="flex justify-center items-center h-96">
          <span className="loading loading-spinner loading-lg"></span>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen pt-20 bg-base-100">
      {/* Main Layout with Side Spaces */}
      <div className="flex min-h-screen">
        <Sidebar/>
        {/* Main Content Area */}
        <main className="flex-1 px-4 sm:px-6 lg:px-8 py-8">
          {/* Header Section */}
          <div className="max-w-7xl mx-auto mb-8">
            <h1 className="text-4xl font-bold text-center mb-4">
              Discover Amazing Games
            </h1>
            <p className="text-xl text-center text-base-content/70 max-w-2xl mx-auto">
              Explore our collection of top-rated video games and find your next adventure
            </p>
          </div>

          {/* Games Grid */}
          <div className="max-w-7xl mx-auto">
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-3 xl:grid-cols-4 2xl:grid-cols-5 gap-6">
              {games.map((game) => (
                <GameCard key={game.id} game={game} />
              ))}
            </div>
          </div>

          {/* Load More Section */}
          <div className="max-w-7xl mx-auto mt-12 text-center">
            <button className="btn btn-primary btn-wide">
              Load More Games
            </button>
          </div>
        </main>

        {/* Right Sidebar Space - Reserved for future use */}
        <aside className="hidden lg:block w-64 xl:w-80">
        </aside>
      </div>
    </div>
  );
};

export default HomeScreen;