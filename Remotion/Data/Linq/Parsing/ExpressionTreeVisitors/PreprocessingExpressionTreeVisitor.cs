// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Parsing.ExpressionTreeVisitors
{
  /// <summary>
  /// Preprocesses an expression tree for parsing. The preprocessing involves detection of sub-queries and VB-specific expressions.
  /// </summary>
  public class PreprocessingExpressionTreeVisitor : ExpressionTreeVisitor
  {
    public static Expression Process (Expression expressionTree, MethodCallExpressionNodeTypeRegistry nodeTypeRegistry)
    {
      ArgumentUtility.CheckNotNull ("expressionTree", expressionTree);
      ArgumentUtility.CheckNotNull ("nodeTypeRegistry", nodeTypeRegistry);

      var visitor = new PreprocessingExpressionTreeVisitor (nodeTypeRegistry);
      return visitor.VisitExpression (expressionTree);
    }

    private readonly MethodCallExpressionNodeTypeRegistry _nodeTypeRegistry;
    private readonly QueryParser _innerParser;

    private PreprocessingExpressionTreeVisitor (MethodCallExpressionNodeTypeRegistry nodeTypeRegistry)
    {
      ArgumentUtility.CheckNotNull ("nodeTypeRegistry", nodeTypeRegistry);

      _nodeTypeRegistry = nodeTypeRegistry;
      _innerParser = new QueryParser (new ExpressionTreeParser (_nodeTypeRegistry, new IExpressionTreeProcessingStep[0]));
    }

    public override Expression VisitExpression (Expression expression)
    {
      var potentialQueryOperatorExpression = _innerParser.ExpressionTreeParser.GetQueryOperatorExpression (expression);
      if (potentialQueryOperatorExpression != null
          && _innerParser.ExpressionTreeParser.NodeTypeRegistry.IsRegistered (potentialQueryOperatorExpression.Method))
        return CreateSubQueryNode (potentialQueryOperatorExpression);
      else
        return base.VisitExpression (expression);
    }
    
    protected internal override Expression VisitUnknownNonExtensionExpression (Expression expression)
    {
      //ignore
      return expression;
    }

    private SubQueryExpression CreateSubQueryNode (MethodCallExpression methodCallExpression)
    {
      QueryModel queryModel = _innerParser.GetParsedQuery (methodCallExpression);
      return new SubQueryExpression (queryModel);
    }
  }
}