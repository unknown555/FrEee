using FrEee.Extensions;
using FrEee.Interfaces;
using FrEee.Objects.LogMessages;
using FrEee.Serialization; using FrEee.Serialization.Attributes;
using System.Collections.Generic;

namespace FrEee.Objects.Civilization.Diplomacy
{
	/// <summary>
	/// An action that accepts a proposal.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class AcceptProposalAction : Action
	{
		public AcceptProposalAction(Proposal proposal)
			: base(proposal.Executor)
		{
			Proposal = proposal;
		}

		public override string Description
		{
			get { return "Reject " + Proposal; }
		}

		/// <summary>
		/// The proposal in question.
		/// </summary>
		[GameReference]
		public Proposal Proposal { get; set; }

		public override void Execute()
		{
			if (Proposal.IsResolved)
				Executor.Log.Add(Target.CreateLogMessage("The proposal \"" + Proposal + "\" has already been resolved and cannot be accepted now.", LogMessageType.Error));
			else
			{
				Target.Log.Add(Executor.CreateLogMessage("The " + Executor + " has accepted our proposal (" + Proposal + ").", LogMessageType.Generic));
				Proposal.Execute();
			}
		}

		public override void ReplaceClientIDs(IDictionary<long, long> idmap, ISet<IPromotable> done = null)
		{
			if (done == null)
				done = new HashSet<IPromotable>();
			if (!done.Contains(this))
			{
				done.Add(this);
				base.ReplaceClientIDs(idmap, done);
				Proposal.ReplaceClientIDs(idmap, done);
			}
		}
	}
}
