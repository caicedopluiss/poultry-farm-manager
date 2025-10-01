namespace PoultryFarmManager.Application.DTOs;

public interface IDtoEntityMapper<TFrom, TTo> where TFrom : class where TTo : class
{
    TTo Map(TFrom from, TTo? to = null);
}
