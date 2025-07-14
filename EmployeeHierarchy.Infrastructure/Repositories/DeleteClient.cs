namespace EmployeeHierarchy.Infrastructure.Repositories;

using System.Data;
using Dapper;
using Domain;
using Utils;
using Domain.Entities;

public class DeleteClient: IDeleteClient
{
    private readonly IDbConnection connection;

    public DeleteClient(IDbConnection connection)
    {
        this.connection = connection;
    }

    public async Task<bool> DeleteEmployeeAndReassignAsync(int employeeId, int deletedByUserId)
    {
        if (this.connection.State != ConnectionState.Open)
        {
            this.connection.Open();
        }

        var parameters = new DynamicParameters();
        parameters.Add("@employee_id", employeeId);
        parameters.Add("@deleted_by_user_id", deletedByUserId);

        await this.connection.ExecuteAsync("sp_delete_employee", parameters, commandType: CommandType.StoredProcedure);

        return true;
    }
}