using Roommates.Models;
using Roommates.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roommates
{
    class Program
    {
        //  This is the address of the database.
        //  We define it here as a constant since it will never change.
        private const string CONNECTION_STRING = @"server=localhost\SQLExpress;database=Roommates;integrated security=true;TrustServerCertificate=true;";

        // I now see the importance of SOLID principles, and will do my best to follow them from now on
        static void Main(string[] args)
        {
            RoomRepository roomRepo = new RoomRepository(CONNECTION_STRING);
            ChoreRepository choreRepo = new ChoreRepository(CONNECTION_STRING);
            RoommateRepository roommateRepo = new RoommateRepository(CONNECTION_STRING);
            RoommateChoreRepository roommateChoreRepo = new RoommateChoreRepository(CONNECTION_STRING);

            List<Room> rooms = roomRepo.GetAll();
            List<Chore> chores = choreRepo.GetAll();
            List<Roommate> roommates = roommateRepo.GetAll();

            bool runProgram = true;
            while (runProgram)
            {
                string selection = GetMenuSelection();

                switch (selection)
                {
                    case ("Show all rooms"):
                        foreach (Room room in rooms)
                        {
                            Console.WriteLine($"{room.Name} has an Id of {room.Id} and a max occupancy of {room.MaxOccupancy}");
                        }
                        Console.Write("Press any key to continue");
                        Console.ReadKey();
                        break;
                    case ("Search for room"):
                        Console.Write("Room Id: ");
                        int id = int.Parse(Console.ReadLine());

                        Room roomById = roomRepo.GetById(id);

                        Console.WriteLine($"{roomById.Id} - {roomById.Name} Max Occupancy({roomById.MaxOccupancy})");
                        Console.Write("Press any key to continue");
                        Console.ReadKey();
                        break;
                    case ("Add a room"):
                        Console.Write("Room name: ");
                        string name = Console.ReadLine();

                        Console.Write("Max occupancy: ");
                        int max = int.Parse(Console.ReadLine());

                        Room roomToAdd = new Room
                        {
                            Name = name,
                            MaxOccupancy = max
                        };

                        roomRepo.Insert(roomToAdd);

                        Console.WriteLine($"{roomToAdd.Name} has been added and assigned an Id of {roomToAdd.Id}");
                        Console.Write("Press any key to continue");
                        Console.ReadKey();
                        break;
                    case ("Show all chores"):
                        Console.WriteLine("Chores:");
                        foreach (Chore chore in chores)
                        {
                            Console.WriteLine($"{chore.Name} has an id of {chore.Id}");
                        }
                        Console.Write("Press any key to continue");
                        Console.ReadKey();
                        break;
                    case ("Search for chore"):
                        Console.Write("Chore Id: ");
                        int choreId = int.Parse(Console.ReadLine());

                        Chore choreById = choreRepo.GetById(choreId);

                        Console.WriteLine($"{choreById.Id} - {choreById.Name}");
                        Console.Write("Press any key to continue");
                        Console.ReadKey();
                        break;
                    case ("Add a chore"):
                        Console.Write("Chore name: ");
                        string choreName = Console.ReadLine();

                        Chore choreToAdd = new Chore
                        {
                            Name = choreName,
                        };

                        choreRepo.Insert(choreToAdd);

                        Console.WriteLine($"{choreToAdd.Name} has been added and assigned an Id of {choreToAdd.Id}");
                        Console.Write("Press any key to continue");
                        Console.ReadKey();
                        break;
                    case ("Search for roommate"):
                        Console.Write("Roommate Id: ");
                        int roommateId = int.Parse(Console.ReadLine());

                        Roommate roommateById = roommateRepo.GetById(roommateId);

                        Console.WriteLine($"NAME: {roommateById.FirstName} - RENT: {roommateById.RentPortion} - ROOM: {roommateById.RoomName}");
                        Console.Write("Press any key to continue");
                        Console.ReadKey();
                        break;
                    case ("Get unassigned chores"):
                        List<Chore> unassignedChores = choreRepo.GetUnassigned();
                        Console.WriteLine("Unassigned Chores:");
                        
                        foreach (Chore chore in unassignedChores)
                        {
                            Console.WriteLine($"ID: {chore.Id} - NAME: {chore.Name}");
                        }

                        Console.Write("Press any key to continue");
                        Console.ReadKey();
                        break;
                    case ("Assign a chore"):
                        // First, have the user select a chore
                        foreach (Chore chore in chores)
                        {
                            Console.WriteLine($"{chore.Id}) {chore.Name}");
                        }

                        Console.WriteLine();
                        Console.WriteLine("Select a chore!");

                        Console.Write("> ");
                        int choreToAssign = int.Parse(Console.ReadLine());

                        // Then, have the user select a roommate to assign it to
                        foreach (Roommate roommate in roommates)
                        {
                            Console.WriteLine($"{roommate.Id}) {roommate.FirstName} {roommate.LastName}");
                        }

                        Console.WriteLine();
                        Console.WriteLine("Select a roommate!");

                        Console.Write("> ");
                        int roommateToAssign = int.Parse(Console.ReadLine());

                        string result = choreRepo.AssignChore(choreToAssign, roommateToAssign) == true ? "Successfully assigned chore!" : "Failed to assign chore.";

                        Console.WriteLine(result);
                        Console.WriteLine();

                        Console.Write("Press any key to continue");
                        Console.ReadKey();
                        break;
                    case ("Get chore counts"):
                        Dictionary<string, int> choreCounts = choreRepo.GetChoreCounts().OrderByDescending(x => x.Value).ToDictionary(x => x.Key,x => x.Value);

                        foreach(KeyValuePair<string, int> x in choreCounts)
                        {
                            Console.WriteLine($"{x.Key}: {x.Value}");
                        }

                        Console.Write("Press any key to continue");
                        Console.ReadKey();
                        break;
                    case ("Update a room"):
                        foreach (Room room in rooms)
                        {
                            Console.WriteLine($"{room.Id} - {room.Name} Max Occupancy({room.MaxOccupancy})");
                        }

                        Console.Write("Which room would you like to update? ");
                        int selectedRoomId = int.Parse(Console.ReadLine());
                        Room selectedRoom = rooms.FirstOrDefault(r => r.Id == selectedRoomId);

                        Console.Write("New Name: ");
                        selectedRoom.Name = Console.ReadLine();

                        Console.Write("New Max Occupancy: ");
                        selectedRoom.MaxOccupancy = int.Parse(Console.ReadLine());

                        roomRepo.Update(selectedRoom);

                        Console.WriteLine("Room has been successfully updated");
                        Console.Write("Press any key to continue");
                        Console.ReadKey();
                        break;
                    case ("Delete a room"):
                        foreach (Room room in rooms)
                        {
                            Console.WriteLine($"{room.Id}) {room.Name}");
                        }

                        Console.WriteLine("Which room should we delete: ");
                        int roomToDelete = int.Parse(Console.ReadLine());

                        roomRepo.Delete(roomToDelete);
                        Console.WriteLine($"Successfully deleted room {roomToDelete}");

                        Console.WriteLine();
                        Console.Write("Press any key to continue");
                        Console.ReadKey();
                        break;
                    case ("Update a chore"):
                        Chore newChore = new Chore();
                        
                        foreach(Chore chore in chores)
                        {
                            Console.WriteLine($"{chore.Id}) {chore.Name}");
                        }

                        Console.Write("Which chore are you updating: ");
                        int choreToUpdate = int.Parse(Console.ReadLine());
                        newChore.Id = choreToUpdate;

                        Console.Write("New name: ");
                        newChore.Name = Console.ReadLine();

                        choreRepo.Update(newChore);

                        Console.WriteLine($"Successfully updated chore {choreToUpdate}");

                        Console.WriteLine();
                        Console.Write("Press any key to continue");
                        Console.ReadKey();
                        break;
                    case ("Delete a chore"):
                        foreach(Chore chore in chores)
                        {
                            Console.WriteLine($"{chore.Id}) {chore.Name}");
                        }

                        Console.WriteLine("Select a chore to delete (DELETIONS ARE FINAL)");
                        Console.Write("> ");
                        int choreToDelete = int.Parse(Console.ReadLine());

                        choreRepo.Delete(choreToDelete);

                        Console.WriteLine($"Successfully deleted chore {choreToDelete}");
                        Console.WriteLine();
                        Console.Write("Press any key to continue");
                        Console.ReadKey();
                        break;
                    case ("Reassign chore"):
                        Console.Clear();

                        List<Chore> assignedChores = choreRepo.GetAssigned();

                        foreach (Chore chore in assignedChores)
                        {
                            Console.WriteLine($"{chore.Id}) {chore.Name}");
                        }

                        Console.WriteLine();
                        Console.WriteLine("Select a chore to reassign");
                        Console.Write("> ");
                        
                        int choreToReassignId = int.Parse(Console.ReadLine());

                        string roommateName = roommateRepo.GetByChore(choreToReassignId);
                        Console.Clear();
                        Console.WriteLine($"This chore is currently assigned to {roommateName}");
                        Console.WriteLine();

                        foreach(Roommate roommate in roommates)
                        {
                            Console.WriteLine($"{roommate.Id}) {roommate.FirstName}");
                        }

                        Console.WriteLine();
                        Console.WriteLine("Who would you like to re-assign it to?");
                        Console.Write("> ");

                        int roommateToAssignId = int.Parse(Console.ReadLine());

                        roommateChoreRepo.Update(roommateToAssignId, choreToReassignId);
                        Console.WriteLine("Successfully updated chore!");
                        Console.WriteLine();

                        Console.Write("Press any key to continue");
                        Console.ReadKey();
                        break;
                    case "List everyone's chores":
                        List<Roommate> roommatesWithChores = roommateRepo.ChoreReport();
                        foreach(Roommate rM in roommatesWithChores)
                        {
                            string rMChores = String.Join(", ", rM.Chores.Select(x => x.Name));
                            Console.WriteLine($"{rM.FirstName}'s Chores: {rMChores}");
                        }

                        Console.WriteLine();
                        Console.Write("Press any key to continue");
                        Console.ReadKey();
                        break;
                    case "Former roommates":
                        foreach(Roommate rM in roommateRepo.Former())
                        {
                            Console.WriteLine($"{rM.FirstName} {rM.LastName} - MoveInDate: {rM.MovedInDate}");
                        }

                        Console.WriteLine();
                        Console.Write("Press any key to continue");
                        Console.ReadKey();
                        break;
                    case "Days of residency":
                        foreach(KeyValuePair<string, int> x in roommateRepo.DaysOfResidency())
                        {
                            Console.WriteLine($"{x.Key}: {x.Value} Days");
                        }

                        Console.WriteLine();
                        Console.Write("Press any key to continue");
                        Console.ReadKey();
                        break;
                    case "Rent due each month (by person)":
                        foreach(Roommate rM in roommateRepo.MonthlyCost())
                        {
                            Console.WriteLine($"{rM.FirstName}: ${rM.RentPortion}");
                        }
                        
                        Console.WriteLine();
                        Console.Write("Press any key to continue");
                        Console.ReadKey();
                        break;
                    case "Total rent paid (by person)":
                        foreach(Roommate rM in roommateRepo.TotalCost())
                        {
                            Console.WriteLine($"{rM.FirstName} - ${rM.TotalRent.ToString("N0")} | Current: {(rM.MovedOutDate == default(DateTime) ? "True" : "False")}");
                        }

                        Console.WriteLine();
                        Console.Write("Press any key to continue");
                        Console.ReadKey();
                        break;
                    case ("Exit"):
                        runProgram = false;
                        break;
                }
            }

        }

        static string GetMenuSelection()
        {
            Console.Clear();

            List<string> options = new List<string>()
            {
                "Show all rooms",
                "Search for room",
                "Add a room",
                "Show all chores",
                "Search for chore",
                "Add a chore",
                "Search for roommate",
                "Get unassigned chores",
                "Assign a chore",
                "Get chore counts",
                "Update a room",
                "Delete a room",
                "Update a chore",
                "Delete a chore",
                "Reassign chore",
                "List everyone's chores",
                "Former roommates",
                "Days of residency",
                "Rent due each month (by person)",
                "Total rent paid (by person)",
                "Exit"
            };

            for (int i = 0; i < options.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {options[i]}");
            }

            while (true)
            {
                try
                {
                    Console.WriteLine();
                    Console.Write("Select an option > ");

                    string input = Console.ReadLine();
                    int index = int.Parse(input) - 1;
                    return options[index];
                }
                catch (Exception)
                {

                    continue;
                }
            }
        }
    }
}