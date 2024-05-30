using Bolao10.Model.Entities;
using Bolao10.Persistence.Repository;
using System;
using System.Collections.Generic;

public class ChatService
{

    ComentarioRepository _comentarioRepository;
    public ChatService()
    {
        _comentarioRepository = new ComentarioRepository();
    }
    public IList<Comentario> GetAll()
    {
        return _comentarioRepository.GetAllFilteredByAndOrderedBy(
                    x => x.ComentarioPai == null && x.CriadoEm.AddHours(-3).Date >= DateTime.Now.AddHours(-3).Date.AddDays(-7),
                    x => -x.CriadoEm.Ticks);
    }
}