using Microsoft.Extensions.DependencyInjection;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Repositories;
using MonsterTradingCardsGame.Repositories.Interfaces;
using MonsterTradingCardsGame.Services;
using System;
using System.Collections.Generic;
using MonsterTradingCardsGame.Enums;
using MonsterTradingCardsGame.Models.DisplayModels;
using Npgsql;
using MonsterTradingCardsGame.Application.Handlers;
using MonsterTradingCardsGame.Application.Handlers.Interfaces;
using MonsterTradingCardsGame.Application.Configurations;
using MonsterTradingCardsGame.Helper.HttpServer;

namespace MonsterTradingCardsGame
{
    internal class Program
    {
        private static BaseHandler _baseHandler;
        static async Task Main(string[] args)
        {
            HttpSvr svr = new HttpSvr();
            var serviceProvider = new ServiceCollection()
                // Register repositories as singletons
                .AddScoped<IUserRepository, UserRepository>()
                .AddScoped<ICardRepository, CardRepository>()
                .AddScoped<IPackageRepository, PackageRepository>()
                .AddScoped<ITradingRepository, TradingRepository>()

                // Register services
                .AddScoped<AuthenticationService>()
                .AddScoped<PackageService>()
                .AddScoped<UserService>()
                .AddScoped<CardService>()
                .AddScoped<GameService>()
                .AddScoped<BattleService>()
                .AddScoped<TradingService>()

                //Register handlers
                .AddScoped<TradingHandler>()
                .AddScoped<BattleHandler>()
                .AddScoped<StatsHandler>()
                .AddScoped<UserHandler>()
                .AddScoped<AuthenticationHandler>()
                .AddScoped<PackageHandler>()
                .AddScoped<TransactionHandler>()
                .AddScoped<CardHandler>()
                .AddScoped<DeckHandler>()
                .AddScoped<ScoreboardHandler>()
                .AddScoped<ApiHandler>()
                .AddScoped<BaseHandler>()


                // Register NpgsqlConnection
                .AddScoped<NpgsqlConnection>(provider =>
                {
                    var connection = new NpgsqlConnection(DatabaseConfig.ConnectionString);
                    return connection;
                })

                // Register JWT configuration
                .AddScoped(provider => new AuthenticationService(
                    provider.GetRequiredService<IUserRepository>(),
                    JWTConfig.JwtSecret,
                    60
                ))

                .BuildServiceProvider();

            //AuthenticationService authenticationService = serviceProvider.GetService<AuthenticationService>();
            //var token = await authenticationService.Login("admin", "istrator");
            //authenticationService.Authenticate(token.token);


            _baseHandler = serviceProvider.GetService<BaseHandler>();
            svr.Incoming += Svr_Incoming;
            svr.Run();

         


        }

        //private static async void Svr_Incoming(object sender, HttpSvrEventArgs e)
        //{
        //    _baseHandler.Handle(e);
        //}

        private static async void Svr_Incoming(object sender, HttpSvrEventArgs e)
        {
            await _baseHandler.Handle(e);
        }
    }
}