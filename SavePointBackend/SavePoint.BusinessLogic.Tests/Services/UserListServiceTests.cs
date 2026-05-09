using AutoMapper;
using SavePoint.BusinessLogic.Services;
using SavePoint.Common.Dtos.Lists;
using SavePoint.Common.Exceptions;
using SavePoint.DAL.Repositories.Interfaces;
using SavePoint.Entities.Common;
using SavePoint.Entities.Games;
using SavePoint.Entities.Lists;

namespace SavePoint.BusinessLogic.Tests.Services
{
    public class UserListServiceTests
    {
        private readonly Mock<IUserListRepository> _listRepoMock;
        private readonly Mock<IGameRepository> _gameRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UserListService _sut;

        public UserListServiceTests()
        {
            _listRepoMock = new Mock<IUserListRepository>();
            _gameRepoMock = new Mock<IGameRepository>();
            _mapperMock = new Mock<IMapper>();
            _sut = new UserListService(_listRepoMock.Object, _gameRepoMock.Object, _mapperMock.Object);
        }

        // ---------- GetListByIdAsync ----------

        [Fact]
        public async Task GetListByIdAsync_WithPublicList_ReturnsMappedDto()
        {
            // Arrange
            var listId = Guid.NewGuid();
            var list = new UserList { Id = listId, UserId = "owner", IsPublic = true, Votes = new List<UserListVote>() };
            var dto = new UserListDto { Id = listId, UserId = "owner", IsPublic = true };
            _listRepoMock.Setup(r => r.GetByIdWithDetailsAsync(listId)).ReturnsAsync(list);
            _mapperMock.Setup(m => m.Map<UserListDto>(list)).Returns(dto);

            // Act
            var result = await _sut.GetListByIdAsync(listId);

            // Assert
            result.Should().BeSameAs(dto);
        }

        [Fact]
        public async Task GetListByIdAsync_WithMissingList_ThrowsNotFoundException()
        {
            // Arrange
            var listId = Guid.NewGuid();
            _listRepoMock.Setup(r => r.GetByIdWithDetailsAsync(listId)).ReturnsAsync((UserList?)null);

            // Act
            Func<Task> act = async () => await _sut.GetListByIdAsync(listId);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetListByIdAsync_WithPrivateListAndDifferentUser_ThrowsForbiddenException()
        {
            // Arrange
            var listId = Guid.NewGuid();
            var list = new UserList { Id = listId, UserId = "owner", IsPublic = false, Votes = new List<UserListVote>() };
            _listRepoMock.Setup(r => r.GetByIdWithDetailsAsync(listId)).ReturnsAsync(list);

            // Act
            Func<Task> act = async () => await _sut.GetListByIdAsync(listId, currentUserId: "intruder");

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task GetListByIdAsync_WithCurrentUserVote_PopulatesCurrentUserVoteOnDto()
        {
            // Arrange
            var listId = Guid.NewGuid();
            var list = new UserList
            {
                Id = listId,
                UserId = "owner",
                IsPublic = true,
                Votes = new List<UserListVote>
                {
                    new UserListVote { UserId = "voter", IsUpvote = true }
                }
            };
            var dto = new UserListDto { Id = listId };
            _listRepoMock.Setup(r => r.GetByIdWithDetailsAsync(listId)).ReturnsAsync(list);
            _mapperMock.Setup(m => m.Map<UserListDto>(list)).Returns(dto);

            // Act
            var result = await _sut.GetListByIdAsync(listId, currentUserId: "voter");

            // Assert
            result.CurrentUserVote.Should().BeTrue();
        }

        // ---------- GetUserListsAsync ----------

        [Fact]
        public async Task GetUserListsAsync_AsNonOwner_FiltersPrivateLists()
        {
            // Arrange
            var ownerId = "owner";
            var publicList = new UserList { Id = Guid.NewGuid(), UserId = ownerId, IsPublic = true, Votes = new List<UserListVote>() };
            var privateList = new UserList { Id = Guid.NewGuid(), UserId = ownerId, IsPublic = false, Votes = new List<UserListVote>() };
            var sourceLists = new List<UserList> { publicList, privateList };
            _listRepoMock.Setup(r => r.GetUserListsAsync(ownerId)).ReturnsAsync(sourceLists);
            _mapperMock
                .Setup(m => m.Map<List<UserListDto>>(It.Is<List<UserList>>(l => l.Count == 1 && l[0].Id == publicList.Id)))
                .Returns(new List<UserListDto> { new UserListDto { Id = publicList.Id } });

            // Act
            var result = await _sut.GetUserListsAsync(ownerId, currentUserId: "viewer");

            // Assert
            result.Should().ContainSingle();
            result[0].Id.Should().Be(publicList.Id);
        }

        // ---------- CreateListAsync ----------

        [Fact]
        public async Task CreateListAsync_WithValidDto_PersistsListWithProvidedUserId()
        {
            // Arrange
            var userId = "u1";
            var createDto = new CreateUserListDto { Name = "MyList", Description = "d", IsPublic = true };
            var entity = new UserList { Name = "MyList", Description = "d", IsPublic = true };
            var dto = new UserListDto { Name = "MyList" };
            _mapperMock.Setup(m => m.Map<UserList>(createDto)).Returns(entity);
            _listRepoMock.Setup(r => r.CreateAsync(It.IsAny<UserList>())).ReturnsAsync((UserList l) => l);
            _mapperMock.Setup(m => m.Map<UserListDto>(It.IsAny<UserList>())).Returns(dto);

            // Act
            var result = await _sut.CreateListAsync(createDto, userId);

            // Assert
            result.Should().BeSameAs(dto);
            _listRepoMock.Verify(r => r.CreateAsync(It.Is<UserList>(l =>
                l.UserId == userId && l.IsDefault == false && l.DefaultListType == null && l.Id != Guid.Empty)),
                Times.Once);
        }

        // ---------- UpdateListAsync ----------

        [Fact]
        public async Task UpdateListAsync_AsOwnerOnCustomList_UpdatesAllFields()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = new UserList { Id = id, UserId = "owner", IsDefault = false, Name = "old", Description = "olddesc", IsPublic = false };
            var updateDto = new UpdateUserListDto { Name = "new", Description = "newdesc", IsPublic = true };
            _listRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
            _listRepoMock.Setup(r => r.UpdateAsync(existing)).ReturnsAsync(existing);
            _mapperMock.Setup(m => m.Map<UserListDto>(existing)).Returns(new UserListDto { Id = id, Name = "new" });

            // Act
            await _sut.UpdateListAsync(id, updateDto, "owner");

            // Assert
            existing.Name.Should().Be("new");
            existing.Description.Should().Be("newdesc");
            existing.IsPublic.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateListAsync_OnDefaultList_OnlyUpdatesDescriptionAndPrivacy()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = new UserList { Id = id, UserId = "owner", IsDefault = true, Name = "Want to Play", Description = "old", IsPublic = false };
            var updateDto = new UpdateUserListDto { Name = "RenameAttempt", Description = "newdesc", IsPublic = true };
            _listRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
            _listRepoMock.Setup(r => r.UpdateAsync(existing)).ReturnsAsync(existing);
            _mapperMock.Setup(m => m.Map<UserListDto>(existing)).Returns(new UserListDto { Id = id });

            // Act
            await _sut.UpdateListAsync(id, updateDto, "owner");

            // Assert
            existing.Name.Should().Be("Want to Play"); // unchanged
            existing.Description.Should().Be("newdesc");
            existing.IsPublic.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateListAsync_AsNonOwner_ThrowsForbiddenException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _listRepoMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(new UserList { Id = id, UserId = "owner" });

            // Act
            Func<Task> act = async () => await _sut.UpdateListAsync(id, new UpdateUserListDto { Name = "x" }, "intruder");

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>();
        }

        // ---------- DeleteListAsync ----------

        [Fact]
        public async Task DeleteListAsync_OnDefaultList_ThrowsBusinessException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _listRepoMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(new UserList { Id = id, UserId = "owner", IsDefault = true });

            // Act
            Func<Task> act = async () => await _sut.DeleteListAsync(id, "owner");

            // Assert
            await act.Should().ThrowAsync<BusinessException>();
            _listRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteListAsync_AsOwnerOnCustomList_CallsRepositoryDelete()
        {
            // Arrange
            var id = Guid.NewGuid();
            _listRepoMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(new UserList { Id = id, UserId = "owner", IsDefault = false });

            // Act
            await _sut.DeleteListAsync(id, "owner");

            // Assert
            _listRepoMock.Verify(r => r.DeleteAsync(id), Times.Once);
        }

        [Fact]
        public async Task DeleteListAsync_WithMissingList_ThrowsNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _listRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((UserList?)null);

            // Act
            Func<Task> act = async () => await _sut.DeleteListAsync(id, "u1");

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        // ---------- AddGameToListAsync ----------

        [Fact]
        public async Task AddGameToListAsync_WithValidInputs_AddsGame()
        {
            // Arrange
            var listId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            _listRepoMock.Setup(r => r.GetByIdAsync(listId))
                .ReturnsAsync(new UserList { Id = listId, UserId = "owner" });
            _gameRepoMock.Setup(r => r.GetByIdWithDetailsAsync(gameId))
                .ReturnsAsync(new Game { Id = gameId, Name = "g" });
            _listRepoMock.Setup(r => r.IsGameInListAsync(listId, gameId)).ReturnsAsync(false);

            // Act
            await _sut.AddGameToListAsync(listId, gameId, "owner");

            // Assert
            _listRepoMock.Verify(r => r.AddGameToListAsync(listId, gameId), Times.Once);
        }

        [Fact]
        public async Task AddGameToListAsync_AsNonOwner_ThrowsForbiddenException()
        {
            // Arrange
            var listId = Guid.NewGuid();
            _listRepoMock.Setup(r => r.GetByIdAsync(listId))
                .ReturnsAsync(new UserList { Id = listId, UserId = "owner" });

            // Act
            Func<Task> act = async () => await _sut.AddGameToListAsync(listId, Guid.NewGuid(), "intruder");

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task AddGameToListAsync_WithMissingGame_ThrowsNotFoundException()
        {
            // Arrange
            var listId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            _listRepoMock.Setup(r => r.GetByIdAsync(listId))
                .ReturnsAsync(new UserList { Id = listId, UserId = "owner" });
            _gameRepoMock.Setup(r => r.GetByIdWithDetailsAsync(gameId)).ReturnsAsync((Game?)null);

            // Act
            Func<Task> act = async () => await _sut.AddGameToListAsync(listId, gameId, "owner");

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .Where(e => e.ResourceType == "Game");
        }

        [Fact]
        public async Task AddGameToListAsync_WhenGameAlreadyInList_ThrowsConflictException()
        {
            // Arrange
            var listId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            _listRepoMock.Setup(r => r.GetByIdAsync(listId))
                .ReturnsAsync(new UserList { Id = listId, UserId = "owner" });
            _gameRepoMock.Setup(r => r.GetByIdWithDetailsAsync(gameId))
                .ReturnsAsync(new Game { Id = gameId, Name = "g" });
            _listRepoMock.Setup(r => r.IsGameInListAsync(listId, gameId)).ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _sut.AddGameToListAsync(listId, gameId, "owner");

            // Assert
            await act.Should().ThrowAsync<ConflictException>();
            _listRepoMock.Verify(r => r.AddGameToListAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        // ---------- VoteOnListAsync ----------

        [Fact]
        public async Task VoteOnListAsync_OnPublicListByOtherUser_RecordsVote()
        {
            // Arrange
            var listId = Guid.NewGuid();
            _listRepoMock.Setup(r => r.GetByIdAsync(listId))
                .ReturnsAsync(new UserList { Id = listId, UserId = "owner", IsPublic = true });

            // Act
            await _sut.VoteOnListAsync(listId, true, "voter");

            // Assert
            _listRepoMock.Verify(r => r.AddOrUpdateVoteAsync(listId, "voter", true), Times.Once);
        }

        [Fact]
        public async Task VoteOnListAsync_OnPrivateList_ThrowsBusinessException()
        {
            // Arrange
            var listId = Guid.NewGuid();
            _listRepoMock.Setup(r => r.GetByIdAsync(listId))
                .ReturnsAsync(new UserList { Id = listId, UserId = "owner", IsPublic = false });

            // Act
            Func<Task> act = async () => await _sut.VoteOnListAsync(listId, true, "voter");

            // Assert
            await act.Should().ThrowAsync<BusinessException>();
        }

        [Fact]
        public async Task VoteOnListAsync_OnOwnList_ThrowsBusinessException()
        {
            // Arrange
            var listId = Guid.NewGuid();
            _listRepoMock.Setup(r => r.GetByIdAsync(listId))
                .ReturnsAsync(new UserList { Id = listId, UserId = "owner", IsPublic = true });

            // Act
            Func<Task> act = async () => await _sut.VoteOnListAsync(listId, true, "owner");

            // Assert
            await act.Should().ThrowAsync<BusinessException>();
        }

        [Fact]
        public async Task VoteOnListAsync_WithMissingList_ThrowsNotFoundException()
        {
            // Arrange
            var listId = Guid.NewGuid();
            _listRepoMock.Setup(r => r.GetByIdAsync(listId)).ReturnsAsync((UserList?)null);

            // Act
            Func<Task> act = async () => await _sut.VoteOnListAsync(listId, true, "u1");

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        // ---------- RemoveVoteAsync ----------

        [Fact]
        public async Task RemoveVoteAsync_WithExistingList_RemovesAndReturnsTrue()
        {
            // Arrange
            var listId = Guid.NewGuid();
            _listRepoMock.Setup(r => r.GetByIdAsync(listId))
                .ReturnsAsync(new UserList { Id = listId, UserId = "owner" });

            // Act
            var result = await _sut.RemoveVoteAsync(listId, "voter");

            // Assert
            result.Should().BeTrue();
            _listRepoMock.Verify(r => r.RemoveVoteAsync(listId, "voter"), Times.Once);
        }

        [Fact]
        public async Task RemoveVoteAsync_WithMissingList_ReturnsFalse()
        {
            // Arrange
            var listId = Guid.NewGuid();
            _listRepoMock.Setup(r => r.GetByIdAsync(listId)).ReturnsAsync((UserList?)null);

            // Act
            var result = await _sut.RemoveVoteAsync(listId, "voter");

            // Assert
            result.Should().BeFalse();
            _listRepoMock.Verify(r => r.RemoveVoteAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        // ---------- CreateDefaultListsForUserAsync ----------

        [Fact]
        public async Task CreateDefaultListsForUserAsync_CreatesWantToPlayAndFinishedLists()
        {
            // Arrange
            var captured = new List<UserList>();
            _listRepoMock
                .Setup(r => r.CreateAsync(It.IsAny<UserList>()))
                .Callback((UserList l) => captured.Add(l))
                .ReturnsAsync((UserList l) => l);

            // Act
            await _sut.CreateDefaultListsForUserAsync("u1");

            // Assert
            captured.Should().HaveCount(2);
            captured.Should().AllSatisfy(l =>
            {
                l.UserId.Should().Be("u1");
                l.IsDefault.Should().BeTrue();
            });
            captured.Select(l => l.DefaultListType).Should().BeEquivalentTo(new[] { "WantToPlay", "Finished" });
        }

        // ---------- GetUserDefaultListAsync ----------

        [Fact]
        public async Task GetUserDefaultListAsync_WithMissingList_ReturnsNull()
        {
            // Arrange
            _listRepoMock
                .Setup(r => r.GetUserDefaultListAsync("u1", "WantToPlay"))
                .ReturnsAsync((UserList?)null);

            // Act
            var result = await _sut.GetUserDefaultListAsync("u1", "WantToPlay");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserDefaultListAsync_PrivateListAccessedByOtherUser_ReturnsNull()
        {
            // Arrange
            var list = new UserList { Id = Guid.NewGuid(), UserId = "owner", IsPublic = false };
            _listRepoMock
                .Setup(r => r.GetUserDefaultListAsync("owner", "WantToPlay"))
                .ReturnsAsync(list);

            // Act
            var result = await _sut.GetUserDefaultListAsync("owner", "WantToPlay", currentUserId: "intruder");

            // Assert
            result.Should().BeNull();
        }

        // ---------- AddGameToWantToPlayAsync ----------

        [Fact]
        public async Task AddGameToWantToPlayAsync_WhenDefaultListMissing_ReturnsFalse()
        {
            // Arrange
            _listRepoMock
                .Setup(r => r.GetUserDefaultListAsync("u1", "WantToPlay"))
                .ReturnsAsync((UserList?)null);

            // Act
            var result = await _sut.AddGameToWantToPlayAsync(Guid.NewGuid(), "u1");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task AddGameToWantToPlayAsync_WhenAddThrows_ReturnsFalse()
        {
            // Arrange — exception inside AddGameToListAsync should be swallowed and return false
            var listId = Guid.NewGuid();
            var defaultList = new UserList { Id = listId, UserId = "u1" };
            _listRepoMock
                .Setup(r => r.GetUserDefaultListAsync("u1", "WantToPlay"))
                .ReturnsAsync(defaultList);
            _listRepoMock.Setup(r => r.GetByIdAsync(listId)).ReturnsAsync((UserList?)null); // triggers NotFound

            // Act
            var result = await _sut.AddGameToWantToPlayAsync(Guid.NewGuid(), "u1");

            // Assert
            result.Should().BeFalse();
        }

        // ---------- MoveGameToFinished ----------

        [Fact]
        public async Task MoveGameToFinished_WhenEitherDefaultListMissing_ReturnsFalse()
        {
            // Arrange
            _listRepoMock
                .Setup(r => r.GetUserDefaultListAsync("u1", "WantToPlay"))
                .ReturnsAsync(new UserList { Id = Guid.NewGuid(), UserId = "u1" });
            _listRepoMock
                .Setup(r => r.GetUserDefaultListAsync("u1", "Finished"))
                .ReturnsAsync((UserList?)null);

            // Act
            var result = await _sut.MoveGameToFinished(Guid.NewGuid(), "u1");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task MoveGameToFinished_WhenGameInWantToPlay_RemovesAndAddsToFinished()
        {
            // Arrange
            var wantId = Guid.NewGuid();
            var finId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            _listRepoMock
                .Setup(r => r.GetUserDefaultListAsync("u1", "WantToPlay"))
                .ReturnsAsync(new UserList { Id = wantId, UserId = "u1" });
            _listRepoMock
                .Setup(r => r.GetUserDefaultListAsync("u1", "Finished"))
                .ReturnsAsync(new UserList { Id = finId, UserId = "u1" });
            _listRepoMock.Setup(r => r.IsGameInListAsync(wantId, gameId)).ReturnsAsync(true);
            _listRepoMock.Setup(r => r.IsGameInListAsync(finId, gameId)).ReturnsAsync(false);

            // Act
            var result = await _sut.MoveGameToFinished(gameId, "u1");

            // Assert
            result.Should().BeTrue();
            _listRepoMock.Verify(r => r.RemoveGameFromListAsync(wantId, gameId), Times.Once);
            _listRepoMock.Verify(r => r.AddGameToListAsync(finId, gameId), Times.Once);
        }

        [Fact]
        public async Task MoveGameToFinished_WhenGameAlreadyInFinishedAndNotInWantToPlay_DoesNotMutate()
        {
            // Arrange
            var wantId = Guid.NewGuid();
            var finId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            _listRepoMock
                .Setup(r => r.GetUserDefaultListAsync("u1", "WantToPlay"))
                .ReturnsAsync(new UserList { Id = wantId, UserId = "u1" });
            _listRepoMock
                .Setup(r => r.GetUserDefaultListAsync("u1", "Finished"))
                .ReturnsAsync(new UserList { Id = finId, UserId = "u1" });
            _listRepoMock.Setup(r => r.IsGameInListAsync(wantId, gameId)).ReturnsAsync(false);
            _listRepoMock.Setup(r => r.IsGameInListAsync(finId, gameId)).ReturnsAsync(true);

            // Act
            var result = await _sut.MoveGameToFinished(gameId, "u1");

            // Assert
            result.Should().BeTrue();
            _listRepoMock.Verify(r => r.RemoveGameFromListAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _listRepoMock.Verify(r => r.AddGameToListAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        // ---------- IsGameInListAsync ----------

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task IsGameInListAsync_DelegatesToRepository(bool expected)
        {
            // Arrange
            var listId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            _listRepoMock.Setup(r => r.IsGameInListAsync(listId, gameId)).ReturnsAsync(expected);

            // Act
            var result = await _sut.IsGameInListAsync(listId, gameId);

            // Assert
            result.Should().Be(expected);
        }

        // ---------- GetPublicListsAsync ----------

        [Fact]
        public async Task GetPublicListsAsync_PassesPagingParametersToRepository()
        {
            // Arrange
            var pagedResult = new PagedResult<UserList>
            {
                Items = new List<UserList>(),
                TotalCount = 0,
                PageNumber = 3,
                PageSize = 5
            };
            _listRepoMock
                .Setup(r => r.GetPublicListsAsync(3, 5, "halo", "votes"))
                .ReturnsAsync(pagedResult);
            _mapperMock
                .Setup(m => m.Map<List<UserListDto>>(pagedResult.Items))
                .Returns(new List<UserListDto>());

            // Act
            var result = await _sut.GetPublicListsAsync(3, 5, "halo", "votes");

            // Assert
            result.PageNumber.Should().Be(3);
            result.PageSize.Should().Be(5);
            _listRepoMock.Verify(r => r.GetPublicListsAsync(3, 5, "halo", "votes"), Times.Once);
        }
    }
}
