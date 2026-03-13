using eShop.Infrastructure.Repositories.Dapper;
using Moq;
using System.Data;

namespace eShop.Infrastructure.Tests.Repositories;

public class DapperUnitOfWorkTests
{
    private readonly Mock<IDbConnection> _connectionMock = new();
    private readonly Mock<IDbTransaction> _transactionMock = new();

    // ── BeginTransaction ─────────────────────────────────────────────────────

    [Fact]
    public void BeginTransaction_ClosedConnection_OpensConnectionBeforeBeginning()
    {
        _connectionMock.Setup(c => c.State).Returns(ConnectionState.Closed);
        _connectionMock.Setup(c => c.BeginTransaction()).Returns(_transactionMock.Object);

        var sut = new DapperUnitOfWork(_connectionMock.Object);
        sut.BeginTransaction();

        _connectionMock.Verify(c => c.Open(), Times.Once);
    }

    [Fact]
    public void BeginTransaction_OpenConnection_DoesNotReopenConnection()
    {
        _connectionMock.Setup(c => c.State).Returns(ConnectionState.Open);
        _connectionMock.Setup(c => c.BeginTransaction()).Returns(_transactionMock.Object);

        var sut = new DapperUnitOfWork(_connectionMock.Object);
        sut.BeginTransaction();

        _connectionMock.Verify(c => c.Open(), Times.Never);
    }

    [Fact]
    public void BeginTransaction_ReturnsActiveTransaction()
    {
        _connectionMock.Setup(c => c.State).Returns(ConnectionState.Open);
        _connectionMock.Setup(c => c.BeginTransaction()).Returns(_transactionMock.Object);

        var sut = new DapperUnitOfWork(_connectionMock.Object);
        var transaction = sut.BeginTransaction();

        Assert.Same(_transactionMock.Object, transaction);
    }

    // ── Commit ───────────────────────────────────────────────────────────────

    [Fact]
    public void Commit_ActiveTransaction_CommitsAndDisposes()
    {
        _connectionMock.Setup(c => c.State).Returns(ConnectionState.Open);
        _connectionMock.Setup(c => c.BeginTransaction()).Returns(_transactionMock.Object);

        var sut = new DapperUnitOfWork(_connectionMock.Object);
        sut.BeginTransaction();
        sut.Commit();

        _transactionMock.Verify(t => t.Commit(), Times.Once);
        _transactionMock.Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public void Commit_NoActiveTransaction_DoesNotThrow()
    {
        var sut = new DapperUnitOfWork(_connectionMock.Object);

        var ex = Record.Exception(() => sut.Commit());

        Assert.Null(ex);
    }

    // ── Rollback ─────────────────────────────────────────────────────────────

    [Fact]
    public void Rollback_ActiveTransaction_RollsBackAndDisposes()
    {
        _connectionMock.Setup(c => c.State).Returns(ConnectionState.Open);
        _connectionMock.Setup(c => c.BeginTransaction()).Returns(_transactionMock.Object);

        var sut = new DapperUnitOfWork(_connectionMock.Object);
        sut.BeginTransaction();
        sut.Rollback();

        _transactionMock.Verify(t => t.Rollback(), Times.Once);
        _transactionMock.Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public void Rollback_NoActiveTransaction_DoesNotThrow()
    {
        var sut = new DapperUnitOfWork(_connectionMock.Object);

        var ex = Record.Exception(() => sut.Rollback());

        Assert.Null(ex);
    }

    // ── Dispose ──────────────────────────────────────────────────────────────

    [Fact]
    public void Dispose_WithActiveTransaction_DisposesTransactionAndConnection()
    {
        _connectionMock.Setup(c => c.State).Returns(ConnectionState.Open);
        _connectionMock.Setup(c => c.BeginTransaction()).Returns(_transactionMock.Object);

        var sut = new DapperUnitOfWork(_connectionMock.Object);
        sut.BeginTransaction();
        sut.Dispose();

        _transactionMock.Verify(t => t.Dispose(), Times.Once);
        _connectionMock.Verify(c => c.Dispose(), Times.Once);
    }

    [Fact]
    public void Dispose_WithoutActiveTransaction_DisposesConnection()
    {
        var sut = new DapperUnitOfWork(_connectionMock.Object);
        sut.Dispose();

        _connectionMock.Verify(c => c.Dispose(), Times.Once);
    }
}
