using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AssetRegistry.Application.Assets;
using AssetRegistry.Domain.Entities;
using AssetRegistry.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AssetRegistry.Infrastructure.Repositories
{
    public class AssetRepository : IAssetRepository
    {
        private readonly AssetRegistryDbContext _db;


    };