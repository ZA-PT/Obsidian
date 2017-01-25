import * as React from "react";
import { Link } from "react-router";
export const CreateScope = (props) => 
<div className="well bs-component col-md-6">
    <form onSubmit={props.onSubmit} className="form-horizontal">
        <fieldset>
            <legend>Create scope</legend>
            <div className="form-group">
                <label className="col-md-2 control-label">Scope Name</label>

                <div className="col-md-10">
                    <input type="text" name="scopeName" className="form-control" onChange={props.onInputChange} value={props.scopeName} placeholder="Scope Name..."/>
                </div>
            </div>
            <div className="form-group">
                <label className="col-md-2 control-label">Display Name</label>

                <div className="col-md-10">
                    <input type="text" name="displayName" className="form-control" onChange={props.onInputChange} value={props.displayName} placeholder="Display Name..."/>
                </div>
            </div>
            <div className="form-group">
                <label className="col-md-2 control-label">Description</label>

                <div className="col-md-10">
                    <input type="text" name="description" className="form-control" onChange={props.onInputChange} value={props.description} placeholder="Description..."/>
                </div>
            </div>
            <div className="form-group">
                <label className="col-md-2 control-label">Claim Types</label>

                <div className="col-md-10">
                    <input type="text" name="claimTypes" className="form-control" onChange={props.onInputChange} value={props.claimTypes} placeholder="Claim Types..."/>
                </div>
            </div>
            <div className="form-group">
                <div className="col-md-10 col-md-offset-2">
                    <Link to="/manage/scopes">
                    <button type="button" className="btn btn-default">Cancel</button>
                    </Link>
                    <button type="submit" className="btn btn-primary">Submit</button>
                </div>
            </div>
        </fieldset>
    </form>
</div>