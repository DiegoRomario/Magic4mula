using M4.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace M4.Infrastructure.Data.Mappings
{
    public class EmailSolicitacaoMapping : IEntityTypeConfiguration<EmailSolicitacao>
    {
        public void Configure(EntityTypeBuilder<EmailSolicitacao> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(p => p.Destinatarios).IsRequired();
            builder.Property(p => p.Enviado).IsRequired();
            builder.Property(p => p.Mensagem).IsRequired();
            builder.Property(p => p.Titulo).IsRequired();
            builder.Property(p => p.DataEnvio).IsRequired();
            builder.ToTable("EmailSolicitacao");

        }
    }
}
