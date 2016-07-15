#if !__MOBILE__

using System.Collections.Generic;
using System.Data;

#if !NET_40
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
#endif

namespace Kirkin.Mapping.Data
{
    /// <summary>
    /// <see cref="IDbCommand"/> mapping extension methods.
    /// </summary>
    public static class DbCommandExtensions
    {
        /// <summary>
        /// Executes reader on the given <see cref="IDbCommand"/>,
        /// executes mapping of each resulting record and streams the result.
        /// Opens the connection associated with the given command if necessary.
        /// </summary>
        /// <typeparam name="TEntity">Desired type that each <see cref="IDataRecord"/> will be mapped to.</typeparam>
        /// <param name="command">The command to be executed.</param>
        /// <param name="mapAllSourceMembers">Determines whether an exception will be thrown if there are unmapped source members.</param>
        /// <param name="mapAllTargetMembers">Determines whether an exception will be thrown if there are unmapped target members.</param>
        public static IEnumerable<TEntity> ExecuteEntities<TEntity>(this IDbCommand command, bool mapAllSourceMembers = false, bool mapAllTargetMembers = true)
            where TEntity : new()
        {
            if (command.Connection != null && command.Connection.State == ConnectionState.Closed) {
                command.Connection.Open();
            }

            using (IDataReader reader = command.ExecuteReader())
            {
                MapperBuilder<IDataRecord, TEntity> builder = MapperBuilder
                    .FromDataReaderOrRecord(reader)
                    .ToType<TEntity>();

                builder.MapAllSourceMembers = mapAllSourceMembers;
                builder.MapAllTargetMembers = mapAllTargetMembers;

                Mapper<IDataRecord, TEntity> mapper = builder.BuildMapper();

                while (reader.Read()) {
                    yield return mapper.Map(reader);
                }
            }
        }

#if !NET_40
        /// <summary>
        /// Executes reader on the given <see cref="DbCommand"/>,
        /// executes mapping of each resulting record and streams the result.
        /// Opens the connection associated with the given command if necessary.
        /// </summary>
        /// <typeparam name="TEntity">Desired type that each <see cref="DbDataRecord"/> will be mapped to.</typeparam>
        /// <param name="command">The command to be executed.</param>
        /// <param name="mapAllSourceMembers">Determines whether an exception will be thrown if there are unmapped source members.</param>
        /// <param name="mapAllTargetMembers">Determines whether an exception will be thrown if there are unmapped target members.</param>
        /// <param name="cancellationToken">Cancellation token to be checked throughout the operation.</param>
        public static async Task<TEntity[]> ExecuteEntitiesAsync<TEntity>(this DbCommand command, // Cannot use IDbCommand as it does not support TPL async.
                                                                          bool mapAllSourceMembers = false, 
                                                                          bool mapAllTargetMembers = true,
                                                                          CancellationToken cancellationToken = default(CancellationToken))
            where TEntity : new()
        {
            if (command.Connection != null && command.Connection.State == ConnectionState.Closed) {
                await command.Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            using (DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                MapperBuilder<IDataRecord, TEntity> builder = MapperBuilder
                    .FromDataReaderOrRecord(reader)
                    .ToType<TEntity>();

                builder.MapAllSourceMembers = mapAllSourceMembers;
                builder.MapAllTargetMembers = mapAllTargetMembers;

                Mapper<IDataRecord, TEntity> mapper = builder.BuildMapper();
                List<TEntity> entities = new List<TEntity>();

                while (await reader.ReadAsync().ConfigureAwait(false)) {
                    entities.Add(mapper.Map(reader));
                }

                return entities.ToArray();
            }
        }
#endif
    }
}

#endif