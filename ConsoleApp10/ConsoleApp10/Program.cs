using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace ConsoleApp10
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<JoinBenchmark>();
            BenchmarkRunner.Run<GroupJoinBenchmark>();
            BenchmarkRunner.Run<JoinWithMultipleKeysBenchmark>();
        }
    }

    public class Employee
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        public int DepartmentId { get; set; }

        public int Rank { get; set; }

        public Department Department { get; set; }

        public Salary Salary { get; set; }

        public List<Equipment> Equipments { get; set; }

        public static List<Employee> CreateDummies()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());

            return Enumerable.Range(1, 100)
                .Select(
                    id => new Employee
                          {
                              Id = id,
                              Name = Guid.NewGuid().ToString(),
                              Age = random.Next(18, 65),
                              DepartmentId = random.Next(1, 11),
                              Rank = random.Next(1, 4)
                          })
                .ToList();
        }
    }

    public class Department
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public static List<Department> CreateDummies()
        {
            return Enumerable.Range(1, 10).Select(id => new Department { Id = id, Name = Guid.NewGuid().ToString() }).ToList();
        }
    }

    public class Salary
    {
        public int DepartmentId { get; set; }

        public int Rank { get; set; }

        public decimal Amount { get; set; }

        public static List<Salary> CreateDummies()
        {
            return Enumerable.Range(1, 10)
                .SelectMany(
                    depId => Enumerable.Range(1, 3)
                        .Select(rank => new Salary { DepartmentId = depId, Rank = rank, Amount = depId * 10000 + rank * 1000 }))
                .ToList();
        }
    }

    public class Equipment
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Custodian { get; set; }

        public static List<Equipment> CreateDummies()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());

            return Enumerable.Range(1, 1000)
                .Select(id => new Equipment { Id = id, Name = Guid.NewGuid().ToString(), Custodian = random.Next(1, 101) })
                .ToList();
        }
    }

    public class JoinBenchmark
    {
        private List<Employee> employees;
        private List<Department> departments;

        [GlobalSetup]
        public void Setup()
        {
            this.employees = Employee.CreateDummies();
            this.departments = Department.CreateDummies();
        }

        [Benchmark]
        public void EmployeesJoinDepartmentsByForEach()
        {
            foreach (var employee in this.employees)
            {
                employee.Department = this.departments.Single(dep => dep.Id == employee.DepartmentId);
            }
        }

        [Benchmark]
        public void EmployeesJoinDepartmentsByJoin()
        {
            this.employees = this.employees.Join(
                    this.departments,
                    emp => emp.DepartmentId,
                    dep => dep.Id,
                    (emp, dep) =>
                        {
                            emp.Department = dep;

                            return emp;
                        })
                .ToList();
        }
    }

    public class GroupJoinBenchmark
    {
        private List<Employee> employees;
        private List<Equipment> equipments;

        [GlobalSetup]
        public void Setup()
        {
            this.employees = Employee.CreateDummies();
            this.equipments = Equipment.CreateDummies();
        }

        [Benchmark]
        public void EmployeesJoinEquipmentsByForEach()
        {
            foreach (var employee in this.employees)
            {
                employee.Equipments = this.equipments.Where(eqp => eqp.Custodian == employee.Id).ToList();
            }
        }

        [Benchmark]
        public void EmployeesJoinEquipmentsByJoin()
        {
            this.employees = this.employees.GroupJoin(
                    this.equipments,
                    emp => emp.Id,
                    eqp => eqp.Custodian,
                    (emp, eqps) =>
                        {
                            emp.Equipments = eqps.ToList();

                            return emp;
                        })
                .ToList();
        }
    }


    public class JoinWithMultipleKeysBenchmark
    {
        private List<Employee> employees;
        private List<Salary> salaries;

        [GlobalSetup]
        public void Setup()
        {
            this.employees = Employee.CreateDummies();
            this.salaries = Salary.CreateDummies();
        }

        [Benchmark]
        public void EmployeesJoinSalarysByForEach()
        {
            foreach (var employee in this.employees)
            {
                employee.Salary = this.salaries.Single(brk => brk.DepartmentId == employee.DepartmentId && brk.Rank == employee.Rank);
            }
        }

        [Benchmark]
        public void EmployeesJoinSalarysByJoin()
        {
            this.employees = this.employees.Join(
                    this.salaries,
                    emp => new { emp.DepartmentId, emp.Rank },
                    slr => new { slr.DepartmentId, slr.Rank },
                    (emp, slr) =>
                        {
                            emp.Salary = slr;

                            return emp;
                        })
                .ToList();
        }
    }
}