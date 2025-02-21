using System;
using System.Collections.Generic;
using System.Linq;
using Final.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Final.Data.Configuration
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            // --------------------------------
            // 1) Define static GUIDs to ensure relationships line up
            // --------------------------------
            var companyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var controlRoomToolId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var compressorToolId = Guid.Parse("33333333-3333-3333-3333-333333333333");

            // --------------------------------
            // 2) Seed Company
            // --------------------------------
            modelBuilder.Entity<Company>().HasData(
                new Company
                {
                    Id = companyId,
                    Name = "Ciceklisogukhavadeposu",
                    BaseTopic = "ciceklisogukhavadeposu"
                }
            );

            // --------------------------------
            // 3) Seed Tools (Control Room, Compressor, etc.)
            // --------------------------------
            modelBuilder.Entity<MqttTool>().HasData(
                new MqttTool
                {
                    Id = controlRoomToolId,
                    CompanyId = companyId,
                    Name = "Control Room",
                    Description = "Main control room for the company"
                },
                new MqttTool
                {
                    Id = compressorToolId,
                    CompanyId = companyId,
                    Name = "Compressor",
                    Description = "Compressor tool"
                }
            );

            // --------------------------------
            // 4) Seed Static Topics
            // --------------------------------
            var topicRoomStatusId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var topicCompressorId = Guid.Parse("66666666-6666-6666-6666-666666666666");
            var topicPwrRqstId = Guid.Parse("77777777-7777-7777-7777-777777777777");
            var topicSetTempId = Guid.Parse("88888888-8888-8888-8888-888888888888");

            modelBuilder.Entity<MqttTopic>().HasData(
                new MqttTopic
                {
                    Id = topicRoomStatusId,
                    MqttToolId = controlRoomToolId,
                    BaseTopic = "ciceklisogukhavadeposu",
                    TopicTemplate = "ciceklisogukhavadeposu/control_room/room{room}/status",
                    DataType = TopicDataType.Float,
                    HowMany = 1
                },
                new MqttTopic
                {
                    Id = topicCompressorId,
                    MqttToolId = compressorToolId,
                    BaseTopic = "ciceklisogukhavadeposu",
                    TopicTemplate = "ciceklisogukhavadeposu/control_room/compressor/status",
                    DataType = TopicDataType.Int64,
                    HowMany = 1
                },
                new MqttTopic
                {
                    Id = topicPwrRqstId,
                    MqttToolId = controlRoomToolId,
                    BaseTopic = "ciceklisogukhavadeposu",
                    TopicTemplate = "pwr_rqst/room{room}/control_room/ciceklisogukhavadeposu",
                    DataType = TopicDataType.Int64,
                    HowMany = 1
                },
                new MqttTopic
                {
                    Id = topicSetTempId,
                    MqttToolId = controlRoomToolId,
                    BaseTopic = "ciceklisogukhavadeposu",
                    TopicTemplate = "set_temp/room{room}/control_room/ciceklisogukhavadeposu",
                    DataType = TopicDataType.Float,
                    HowMany = 1
                }
            );

            // --------------------------------
            // 5) Seed 12 Room Temp Topics
            // --------------------------------
            var roomTempTopics = new List<MqttTopic>();
            for (int room = 1; room <= 12; room++)
            {
                string hexRoomString = room.ToString("X12"); // 12-digit hexadecimal
                Guid topicId = Guid.Parse($"00000000-0000-0000-0000-{hexRoomString}");

                roomTempTopics.Add(new MqttTopic
                {
                    Id = topicId,
                    MqttToolId = controlRoomToolId,
                    BaseTopic = "ciceklisogukhavadeposu",
                    TopicTemplate = $"ciceklisogukhavadeposu/control_room/room{room}/temp",
                    DataType = TopicDataType.Float,
                    HowMany = 1
                });
            }
            modelBuilder.Entity<MqttTopic>().HasData(roomTempTopics.ToArray());

            // --------------------------------
            // 6) Seed Role for the Company and Bind It
            // --------------------------------
            // Define a stable role ID for the company's role.
            var roleId = "role-ciceklisogukhavadeposu";
            
            // Seed the IdentityRole. Note: IdentityRole's primary key is a string.
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = roleId,
                    Name = "CiceklisogukhavadeposuRole",
                    NormalizedName = "CICEKLISOGUKHAVADEPOSUROLE"
                }
            );

            // Seed the join entity binding the company to this role.
            modelBuilder.Entity<CompanyRole>().HasData(
                new CompanyRole
                {
                    Id = 1,
                    CompanyId = companyId,
                    RoleId = roleId
                }
            );
        }
    }
}
