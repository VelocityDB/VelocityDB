using System;
using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints
{
    /// <summary>
    ///     Features provides a listing of the features/qualities/quirks associated with any Graph implementation.
    ///     This feature listing can be used to dynamically adjust code to the features of the graph implementation.
    ///     For example, this feature listing is used extensively throughout the Blueprints TestSuite to validate behavior of the implementation.
    /// </summary>
    public class Features
    {
        private bool? _ignoresSuppliedIds;
        private bool? _isPersistent;
        private bool? _isRdfModel;
        private bool? _isWrapper;
        private bool? _supportsBooleanProperty;
        private bool? _supportsDoubleProperty;
        private bool? _supportsDuplicateEdges;
        private bool? _supportsEdgeIndex;
        private bool? _supportsEdgeIteration;
        private bool? _supportsEdgeKeyIndex;
        private bool? _supportsEdgeProperties;
        private bool? _supportsEdgeRetrieval;
        private bool? _supportsFloatProperty;
        private bool? _supportsIdProperty;
        private bool? _supportsIndices;
        private bool? _supportsIntegerProperty;
        private bool? _supportsKeyIndices;
        private bool? _supportsLabelProperty;
        private bool? _supportsLongProperty;
        private bool? _supportsMapProperty;
        private bool? _supportsMixedListProperty;
        private bool? _supportsPrimitiveArrayProperty;
        private bool? _supportsSelfLoops;
        private bool? _supportsSerializableObjectProperty;
        private bool? _supportsStringProperty;
        private bool? _supportsThreadedTransactions;
        private bool? _supportsTransactions;
        private bool? _supportsUniformListProperty;
        private bool? _supportsVertexIndex;
        private bool? _supportsVertexIteration;
        private bool? _supportsVertexKeyIndex;
        private bool? _supportsVertexProperties;

        /// <summary>
        ///     Does the graph allow for two edges with the same vertices and edge label to exist?
        /// </summary>
        public bool SupportsDuplicateEdges
        {
            get { return _supportsDuplicateEdges != null && _supportsDuplicateEdges.Value; }
            set { _supportsDuplicateEdges = value; }
        }

        /// <summary>
        ///     Does the graph allow an edge to have the same out/tail and in/head vertex?
        /// </summary>
        public bool SupportsSelfLoops
        {
            get { return _supportsSelfLoops != null && _supportsSelfLoops.Value; }
            set { _supportsSelfLoops = value; }
        }

        /// <summary>
        ///     Does the graph allow any serializable object to be used as a property value for a graph element?
        /// </summary>
        public bool SupportsSerializableObjectProperty
        {
            get { return _supportsSerializableObjectProperty != null && _supportsSerializableObjectProperty.Value; }
            set { _supportsSerializableObjectProperty = value; }
        }

        /// <summary>
        ///     Does the graph allows boolean to be used as a property value for a graph element?
        /// </summary>
        public bool SupportsBooleanProperty
        {
            get { return _supportsBooleanProperty != null && _supportsBooleanProperty.Value; }
            set { _supportsBooleanProperty = value; }
        }

        /// <summary>
        ///     Does the graph allows double to be used as a property value for a graph element?
        /// </summary>
        public bool SupportsDoubleProperty
        {
            get { return _supportsDoubleProperty != null && _supportsDoubleProperty.Value; }
            set { _supportsDoubleProperty = value; }
        }

        /// <summary>
        ///     Does the graph allows float to be used as a property value for a graph element?
        /// </summary>
        public bool SupportsFloatProperty
        {
            get { return _supportsFloatProperty != null && _supportsFloatProperty.Value; }
            set { _supportsFloatProperty = value; }
        }

        /// <summary>
        ///     Does the graph allows integer to be used as a property value for a graph element?
        /// </summary>
        public bool SupportsIntegerProperty
        {
            get { return _supportsIntegerProperty != null && _supportsIntegerProperty.Value; }
            set { _supportsIntegerProperty = value; }
        }

        /// <summary>
        ///     Does the graph allows a primitive array to be used as a property value for a graph element?
        /// </summary>
        public bool SupportsPrimitiveArrayProperty
        {
            get { return _supportsPrimitiveArrayProperty != null && _supportsPrimitiveArrayProperty.Value; }
            set { _supportsPrimitiveArrayProperty = value; }
        }

        /// <summary>
        ///     Does the graph allows list (all objects with the list have the same data types) to be used as a property value for a graph element?
        /// </summary>
        public bool SupportsUniformListProperty
        {
            get { return _supportsUniformListProperty != null && _supportsUniformListProperty.Value; }
            set { _supportsUniformListProperty = value; }
        }

        /// <summary>
        ///     Does the graph allows a mixed list (different data types within the same list) to be used as a
        ///     property value for a graph element?
        /// </summary>
        public bool SupportsMixedListProperty
        {
            get { return _supportsMixedListProperty != null && _supportsMixedListProperty.Value; }
            set { _supportsMixedListProperty = value; }
        }

        /// <summary>
        ///     Does the graph allows long to be used as a property value for a graph element?
        /// </summary>
        public bool SupportsLongProperty
        {
            get { return _supportsLongProperty != null && _supportsLongProperty.Value; }
            set { _supportsLongProperty = value; }
        }

        /// <summary>
        ///     Does the graph allows map to be used as a property value for a graph element?
        /// </summary>
        public bool SupportsMapProperty
        {
            get { return _supportsMapProperty != null && _supportsMapProperty.Value; }
            set { _supportsMapProperty = value; }
        }

        /// <summary>
        ///     Graph allows string to be used as a property value for a graph element.
        /// </summary>
        public bool SupportsStringProperty
        {
            get { return _supportsStringProperty != null && _supportsStringProperty.Value; }
            set { _supportsStringProperty = value; }
        }

        /// <summary>
        ///     Does the graph ignore user provided ids in graph.addVertex(object id)?
        /// </summary>
        public bool IgnoresSuppliedIds
        {
            get { return _ignoresSuppliedIds != null && _ignoresSuppliedIds.Value; }
            set { _ignoresSuppliedIds = value; }
        }

        /// <summary>
        ///     Does the graph persist the graph to disk after shutdown?
        /// </summary>
        public bool IsPersistent
        {
            get { return _isPersistent != null && _isPersistent.Value; }
            set { _isPersistent = value; }
        }

        /// <summary>
        ///     Is the graph an RDF framework?
        ///     Deprecated thus far, isRDFModel describes a collection of features. Use actual features to describe your data model.
        /// </summary>
        public bool IsRdfModel
        {
            get { return _isRdfModel != null && _isRdfModel.Value; }
            set { _isRdfModel = value; }
        }

        /// <summary>
        ///     Does the graph implement WrapperGraph?
        /// </summary>
        public bool IsWrapper
        {
            get { return _isWrapper != null && _isWrapper.Value; }
            set { _isWrapper = value; }
        }

        /// <summary>
        ///     Does the graph implement IIndexableGraph?
        /// </summary>
        public bool SupportsIndices
        {
            get { return _supportsIndices != null && _supportsIndices.Value; }
            set { _supportsIndices = value; }
        }

        /// <summary>
        ///     Does the graph support the indexing of vertices by their properties?
        /// </summary>
        public bool SupportsVertexIndex
        {
            get { return _supportsVertexIndex != null && _supportsVertexIndex.Value; }
            set { _supportsVertexIndex = value; }
        }

        /// <summary>
        ///     Does the graph support the indexing of edges by their properties?
        /// </summary>
        public bool SupportsEdgeIndex
        {
            get { return _supportsEdgeIndex != null && _supportsEdgeIndex.Value; }
            set { _supportsEdgeIndex = value; }
        }

        /// <summary>
        ///     Does the graph implement KeyIndexableGraph?
        /// </summary>
        public bool SupportsKeyIndices
        {
            get { return _supportsKeyIndices != null && _supportsKeyIndices.Value; }
            set { _supportsKeyIndices = value; }
        }

        /// <summary>
        ///     Does the graph support key indexing on vertices?
        /// </summary>
        public bool SupportsVertexKeyIndex
        {
            get { return _supportsVertexKeyIndex != null && _supportsVertexKeyIndex.Value; }
            set { _supportsVertexKeyIndex = value; }
        }

        /// <summary>
        ///     Does the graph support key indexing on edges?
        /// </summary>
        public bool SupportsEdgeKeyIndex
        {
            get { return _supportsEdgeKeyIndex != null && _supportsEdgeKeyIndex.Value; }
            set { _supportsEdgeKeyIndex = value; }
        }

        /// <summary>
        ///     Does the graph support graph.getEdges()?
        /// </summary>
        public bool SupportsEdgeIteration
        {
            get { return _supportsEdgeIteration != null && _supportsEdgeIteration.Value; }
            set { _supportsEdgeIteration = value; }
        }

        /// <summary>
        ///     Does the graph support graph.getVertices()?
        /// </summary>
        public bool SupportsVertexIteration
        {
            get { return _supportsVertexIteration != null && _supportsVertexIteration.Value; }
            set { _supportsVertexIteration = value; }
        }

        /// <summary>
        ///     Does the graph support retrieving edges by id, i.e. graph.getEdge(object id)?
        /// </summary>
        public bool SupportsEdgeRetrieval
        {
            get { return _supportsEdgeRetrieval != null && _supportsEdgeRetrieval.Value; }
            set { _supportsEdgeRetrieval = value; }
        }

        /// <summary>
        ///     Does the graph support setting and retrieving properties on vertices?
        /// </summary>
        public bool SupportsVertexProperties
        {
            get { return _supportsVertexProperties != null && _supportsVertexProperties.Value; }
            set { _supportsVertexProperties = value; }
        }

        /// <summary>
        ///     Does the graph support setting and retrieving properties on edges?
        /// </summary>
        public bool SupportsEdgeProperties
        {
            get { return _supportsEdgeProperties != null && _supportsEdgeProperties.Value; }
            set { _supportsEdgeProperties = value; }
        }

        /// <summary>
        ///     Does the graph implement TransactionalGraph?
        /// </summary>
        public bool SupportsTransactions
        {
            get { return _supportsTransactions != null && _supportsTransactions.Value; }
            set { _supportsTransactions = value; }
        }

        /// <summary>
        ///     Does the graph implement ThreadedTransactionalGraph?
        /// </summary>
        public bool SupportsThreadedTransactions
        {
            get { return _supportsThreadedTransactions != null && _supportsThreadedTransactions.Value; }
            set { _supportsThreadedTransactions = value; }
        }

        /// <summary>
        ///     Does the graph allow the use of the 'id' property name?
        /// </summary>
        public bool SupportsIdProperty
        {
            get { return _supportsIdProperty != null && _supportsIdProperty.Value; }
            set { _supportsIdProperty = value; }
        }

        /// <summary>
        ///     Does the graph allow the use of the 'label' property name?
        /// </summary>
        public bool SupportsLabelProperty
        {
            get { return _supportsLabelProperty != null && _supportsLabelProperty.Value; }
            set { _supportsLabelProperty = value; }
        }

        /// <summary>
        ///     Checks whether the graph supports both vertex and edge properties
        /// </summary>
        /// <returns>whether the graph supports both vertex and edge properties</returns>
        public bool SupportsElementProperties()
        {
            return SupportsVertexProperties && SupportsEdgeProperties;
        }

        public override string ToString()
        {
            var fields = GetType()
                .GetProperties()
                .Select(t => string.Format("{0}: {1}", t.Name, t.GetValue(this, null)));

            return string.Join("\n", fields);
        }

        public IDictionary<string, object> ToMap()
        {
            var fields = GetType()
                .GetProperties()
                .ToDictionary(t => t.Name, t => t.GetValue(this, null));

            return fields;
        }

        /// <summary>
        ///     This method determines whether the full gamut of features have been set by the Graph implementation.
        ///     This is useful for implementers to ensure that they did not miss specifying a feature.
        ///     Throws InvalidOperationException if a feature was not set
        /// </summary>
        public void CheckCompliance()
        {
            var notCompliant = GetType()
                .GetFields()
                .FirstOrDefault(t => t.GetValue(this) == null);

            if (notCompliant != null)
                throw new InvalidOperationException(string.Format("The feature {0} was not specified", notCompliant.Name));
        }

        /// <summary>
        ///     This method copies the features in this features object to another feature object.
        /// </summary>
        /// <returns>a feature object with a clone of the features in the prior.</returns>
        public Features CopyFeatures()
        {
            return MemberwiseClone() as Features;
        }
    }
}