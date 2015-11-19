using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace QuickStart
{

  class QuickStart
  {
    static readonly string systemDir = "QuickStart"; // appended to SessionBase.BaseDatabasePath

    static int Main(string[] args)
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        Console.WriteLine("Running with databases in directory: " + session.SystemDirectory);
        try
        {
          session.BeginUpdate();
          Company company = new Company();
          company.Name = "MyCompany";
          session.Persist(company);
          Employee employee1 = new Employee();
          employee1.Employer = company;
          employee1.FirstName = "John";
          employee1.LastName = "Walter";
          session.Persist(employee1);
          session.Commit();
        }
        catch (Exception ex)
        {
          Trace.WriteLine(ex.Message);
          session.Abort();
        }
      }
      Retrieve();
      return 0;
    }

    static void Retrieve()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        // get all companies from database
        IEnumerable<Company> companies = session.AllObjects<Company>();

        // get all employees that FirstName contains "John"
        IEnumerable<Employee> employees = session.AllObjects<Employee>();
        var query = from Employee emp in employees
                    where emp.FirstName.Contains("John")
                    select emp;

        foreach (Employee emp in query)
        {
          //do something with employee
          emp.ToString();
        }

        // get all Employees that are over 30 years old and are from Berlin:
        var query2 = from Employee emp in employees
                     where emp.Age > 30 && emp.City == "Berlin"
                     select emp;

        foreach (Employee emp in query2)
        {
          //do something with employee
          emp.ToString();
        }
        // get all Employees that work at "MyCompany" company:
        var query3 = from Employee emp in employees
                     where emp.Employer.Name == "MyCompany"
                     select new { emp.FirstName, emp.LastName };
        session.Commit();
      }
    }
  }

  public class Employee : OptimizedPersistable  // This base class contains the object identifier and some helpful properties and functions.
  {
    Company m_company;
    string m_firstName;
    string m_lastName;
    int m_age;
    DateTime m_hireDate;
    string m_city;

    public Company Employer
    {
      get
      {
        return m_company;
      }
      set
      {
        Update(); // required for VelocityDB to know you want to persistently update an object and its possible indices
        m_company = value;
      }
    }

    public string LastName
    {
      get
      {
        return m_lastName;
      }
      set
      {
        Update();
        m_lastName = value;
      }
    }
    public string FirstName
    {
      get
      {
        return m_firstName;
      }
      set
      {
        Update();
        m_firstName = value;
      }
    }
    public int Age
    {
      get
      {
        return m_age;
      }
      set
      {
        Update();
        m_age = value;
      }
    }
    public DateTime HireDate
    {
      get
      {
        return m_hireDate;
      }
      set
      {
        Update();
        m_hireDate = value;
      }
    }
    public string City
    {
      get
      {
        return m_city;
      }
      set
      {
        Update();
        m_city = value;
      }
    }
  }

  public class Company : OptimizedPersistable // This base class contains the object identifier and some helpful properties and functions.
  {
    string m_name;
    string m_address;
    string m_phone;
    public string Name
    {
      get
      {
        return m_name;
      }
      set
      {
        Update(); // required for VelocityDB to know you want to persistently update an object and its possible indices
        m_name = value;
      }
    }
    public string Address
    {
      get
      {
        return m_address;
      }
      set
      {
        Update();
        m_address = value;
      }
    }
    public string Phone
    {
      get
      {
        return m_phone;
      }
      set
      {
        Update();
        m_phone = value;
      }
    }
  }
}
